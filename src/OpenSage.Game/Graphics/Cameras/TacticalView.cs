#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using ImGuiNET;
using OpenSage.Input.Cursors;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Scripting;
using OpenSage.Terrain;

namespace OpenSage.Graphics.Cameras;

/// <summary>
/// The camera controller; used for both RTS gameplay and cutscenes.
/// Based on W3DView and View classes from Generals.
/// Named after TheTacticalView singleton from Generals.
/// </summary>
public sealed class TacticalView : IPersistableObject
{
    public readonly LookAtTranslator LookAtTranslator;

    private readonly GameData _gameData;
    internal readonly Camera Camera;
    private readonly TerrainLogic _terrainLogic;

    private float _defaultPitchAngle;
    private float _pitchAngle;
    public float Pitch
    {
        get => _pitchAngle;
        // Combination of View::setPitch and W3DView::setPitch
        set
        {
            var limit = MathF.PI / 5.0f;
            _pitchAngle = Math.Clamp(value, -limit, limit);

            // In C++ this didn't reset _cameraArrivedAtWaypointOnPathFlag
            ResetAnimationsAndFlags();
            SetCameraTransform();
        }
    }

    /// <summary>
    /// Additional pitch angle, used for special effects.
    /// </summary>
    private float _fxPitch;

    // In C++ these are initialised in Init()
    // TODO: We should make these configurable via INI
    private readonly float _minZoom = 0.2f;
    private readonly float _maxZoom = 1.3f;
    public bool IsZoomLimited { get; set; } = true;
    private float _zoom = 1;
    public float Zoom
    {
        get => _zoom;
        // Combination of View::setZoom and W3DView::setZoom
        set
        {
            _zoom = Math.Clamp(value, _minZoom, _maxZoom);
            ResetAnimationsAndFlags();
            SetCameraTransform();
        }
    }

    private readonly float _defaultAngle = 0.0f;
    /// <summary>
    /// The current yaw of the camera.
    /// </summary>
    private float _angle;
    public float Angle
    {
        get => _angle;
        // Combination of View::setAngle and W3DView::setAngle
        set
        {
            // Normalize angle to +-PI
            _angle = NormalizeAngle(value);
            ResetAnimationsAndFlags();
            SetCameraTransform();
        }
    }

    private float _groundLevel = 10.0f;
    private float _heightAboveGround;
    private float _minHeightAboveGround;
    private float _maxHeightAboveGround;

    public void SetLookDirection(Vector3 lookDirection)
    {
        _angle = MathF.Atan2(lookDirection.Y, lookDirection.X);
    }
    public Vector3 GetLookDirection()
    {
        return new Vector3(
            MathF.Cos(_angle),
            MathF.Sin(_angle),
            0);
    }

    /// <summary>
    /// The current target position of the camera;
    /// what the camera is looking at.
    /// </summary>
    private Vector3 _pos;

    // TODO(Port): Seems like this is only used for motion blur calculations - is this relevant for us?
    private Vector3? _previousLookAtPosition;

    private Vector3 _cameraOffset;

    public Vector3 Position => _pos;

    /// <summary>
    /// The current time multiplier.
    /// TOOD(Port): It is currently unknown whether this is actually used by the game.
    /// </summary>
    public int TimeMultiplier { get; set; } = 1;

    private Vector2 _scrollAmount;
    private readonly float _scrollAmountCutoff;

    // Original has separate m_doingMoveCameraOnWaypointPath and m_mcwpInfo fields,
    // but let's use a nullable field instead.
    private MoveAlongWaypointPathInfo? _path;
    // TODO(Port): In ZH this is set, but never actually used?
    private bool _cameraArrivedAtWaypointOnPathFlag;
    [MemberNotNullWhen(true, nameof(_path))]
    private bool DoingMoveCameraOnWaypointPath => _path != null;

    private RotateCameraInfo? _rotateAnimation;
    [MemberNotNullWhen(true, nameof(_rotateAnimation))]
    private bool DoingRotateCamera => _rotateAnimation != null;

    private PitchCameraInfo? _pitchAnimation;
    [MemberNotNullWhen(true, nameof(_pitchAnimation))]
    private bool DoingPitchCamera => _pitchAnimation != null;

    private ZoomCameraInfo? _zoomAnimation;
    [MemberNotNullWhen(true, nameof(_zoomAnimation))]
    private bool DoingZoomCamera => _zoomAnimation != null;

    private bool _doingScriptedCameraLock;

    private RectangleF _cameraConstraint;
    private bool _cameraConstraintValid;

    // These should only be modified by BuildCameraTransform
    // They are here so that we can show them in the inspector
    private Vector3 _sourcePos;
    private Vector3 _targetPos;

    public GameObject? CameraLock { get; set; }
    private bool _snapImmediate;
    private float _lockDist;
    private LockType _lockType;

    // TODO(Port): Disable WND input when this is true
    public bool MouseLock { get; set; }

    public bool IsCameraMovementFinished
    {
        get
        {
            // TODO(Port): What to do with view filters?
            return !DoingMoveCameraOnWaypointPath &&
                   !DoingRotateCamera &&
                   !DoingPitchCamera &&
                   !DoingZoomCamera;
        }
    }

    public TacticalView(GameData gameData, Camera camera, TerrainLogic terrainLogic, CursorManager cursorManager)
    {
        _gameData = gameData;
        Camera = camera;
        _terrainLogic = terrainLogic;
        _scrollAmountCutoff = _gameData.ScrollAmountCutoff;
        LookAtTranslator = new LookAtTranslator(this, cursorManager, gameData);

        Init();
        SetDefaultView(0, 0, 1);

        // This should be called every time a new map is loaded
        // Since we re-create CameraController every time a new map is loaded, this should be basically the same
        InitHeightForMap();
        SetAngleAndPitchToDefault();
        SetZoomToDefault();
    }

    void Init()
    {
        _pos = Vector3.Zero;
        _angle = 0.0f;
        var defaultLookAtPoint = new Vector3(
            87.0f,
            77.0f,
            0.0f
        ) * HeightMap.HorizontalScale;
        _pos = defaultLookAtPoint;
        _maxHeightAboveGround = _gameData.MaxCameraHeight;
        _minHeightAboveGround = _gameData.MinCameraHeight;

        SetCameraTransform();
    }

    public void InitHeightForMap()
    {
        _groundLevel = _terrainLogic.GetGroundHeight(_pos.X, _pos.Y);
        const float MaxGroundLevel = 120.0f;
        _groundLevel = Math.Min(_groundLevel, MaxGroundLevel);

        _cameraOffset.Z = _groundLevel + _gameData.CameraHeight;
        UpdateCameraOffsetBasedOnZ();
        _cameraConstraintValid = false;
        SetCameraTransform();
    }

    /// <summary>
    /// Calculates new camera offset based on its current Z value and default pitch and yaw angles.
    /// </summary>
    private void UpdateCameraOffsetBasedOnZ()
    {
        _cameraOffset.Y = -(_cameraOffset.Z / MathF.Tan(_gameData.CameraPitch * (MathF.PI / 180.0f)));
        _cameraOffset.X = -(_cameraOffset.Y * MathF.Tan(_gameData.CameraYaw * (MathF.PI / 180.0f)));
    }

    public void SetDefaultView(float pitch, float angle, float maxHeight)
    {
        // Yup, angle is unused in the C++ version as well
        _defaultPitchAngle = pitch;
        _maxHeightAboveGround = _gameData.MaxCameraHeight * maxHeight;
        if (_minHeightAboveGround > _maxHeightAboveGround)
        {
            _maxHeightAboveGround = _minHeightAboveGround;
        }
    }

    public void SetAngleAndPitchToDefault()
    {
        // From View.cpp
        _angle = _defaultAngle;
        _pitchAngle = _defaultPitchAngle;
        // From W3DView.cpp
        _fxPitch = 1.0f;
        SetCameraTransform();
    }

    // In C++ this code was repeated multiple times, and what exactly was reset varied
    // Hopefully those differences weren't important
    void ResetAnimationsAndFlags()
    {
        _path = null;
        _pitchAnimation = null;
        _rotateAnimation = null;
        _zoomAnimation = null;
        _cameraArrivedAtWaypointOnPathFlag = false;
        _doingScriptedCameraLock = false;
        _cameraConstraintValid = false;
    }

    public void SetZoomToDefault()
    {
        // terrain height + desired height offset == cameraOffset * actual zoom
        // find best approximation of max terrain height we can see
        var terrainHeightMax = GetHeightAroundPos(_pos.Vector2XY());
        var desiredHeight = terrainHeightMax + _maxHeightAboveGround;
        var desiredZoom = desiredHeight / _cameraOffset.Z;

        // There's a small bug here, matching the original code: initial zoom value is not clamped,
        // and at least in Alpine Assault the initial value is slightly above the max zoom value.
        _zoom = desiredZoom;
        _heightAboveGround = _maxHeightAboveGround;

        ResetAnimationsAndFlags();
        SetCameraTransform();
    }

    float GetHeightAroundPos(Vector2 pos)
    {
        const float terrainSampleSize = 40.0f;

        var terrainHeight = _terrainLogic.GetGroundHeight(pos.X, pos.Y);
        var terrainHeightMax = terrainHeight;

        ReadOnlySpan<Vector2> sampleOffsets = [
            new Vector2(1, -1),
            new Vector2(-1, -1),
            new Vector2(1, 1),
            new Vector2(-1, 1),
        ];

        foreach (var offset in sampleOffsets)
        {
            var samplePos = pos + offset * terrainSampleSize;
            var sample = _terrainLogic.GetGroundHeight(samplePos.X, samplePos.Y);
            terrainHeightMax = Math.Max(terrainHeightMax, sample);
        }
        return terrainHeightMax;
    }

    public void LookAt(Vector3 pos)
    {
        // TODO(Port): This didn't get run in normal skirmish maps, so maybe it's only necessary for SP missions?
        // if (o->z > PATHFIND_CELL_SIZE_F + TheTerrainLogic->getGroundHeight(pos.x, pos.y))
        // {
        //     // Pos.z is not used, so if we want to look at something off the ground,
        //     // we have to look at the spot on the ground such that the object intersects
        //     // with the look at vector in the center of the screen.  jba.
        //     Vector3 rayStart, rayEnd;
        //     LineSegClass lineseg;
        //     CastResultStruct result;
        //     Vector3 intersection(0, 0, 0);
        //
        //     rayStart = m_3DCamera->Get_Position();                             // get camera location
        //     m_3DCamera->Un_Project(rayEnd, Vector2(0.0f, 0.0f)); // get world space point
        //     rayEnd -= rayStart;                                                                     // vector camera to world space point
        //     rayEnd.Normalize();                                                                     // make unit vector
        //     rayEnd *= m_3DCamera->Get_Depth();                                     // adjust length to reach far clip plane
        //     rayStart.Set(pos.x, pos.y, pos.z);
        //     rayEnd += rayStart; // get point on far clip plane along ray from camera.
        //     lineseg.Set(rayStart, rayEnd);
        //
        //     RayCollisionTestClass raytest(lineseg, &result);
        //
        //     if (TheTerrainRenderObject->Cast_Ray(raytest))
        //     {
        //         // get the point of intersection according to W3D
        //         pos.x = result.ContactPoint.X;
        //         pos.y = result.ContactPoint.Y;
        //
        //     } // end if
        // }

        pos.Z = 0.0f;
        _pos = pos;
        ResetAnimationsAndFlags();
        SetCameraTransform();
    }

    public void SetPitch(float pitch)
    {
        Pitch = pitch;
    }

    [MemberNotNull(nameof(_path))]
    internal void MoveCameraTo(Vector3 pos, double milliseconds, int shutter, bool orient, float easeIn, float easeOut)
    {
        _path = new MoveAlongWaypointPathInfo();
        _path.Waypoints[0] = _pos;
        _path.CameraAngle[0] = _angle;
        _path.WaySegLength[0] = 0;

        _path.Waypoints[1] = pos;
        _path.WaySegLength[1] = 0;

        _path.Waypoints[2] = pos;
        _path.WaySegLength[2] = 0;

        _path.NumWaypoints = 2;

        milliseconds = Math.Max(milliseconds, 1);
        _path.TotalTimeMilliseconds = milliseconds;
        _path.Shutter = shutter;
        _path.CurSegment = 1;
        _path.CurSegDistance = 0;
        _path.TotalDistance = 0;

        SetupWaypointPath(_path, orient);
        if (_path.TotalTimeMilliseconds == 1)
        {
            // Do it instantly
            var oneMillisecond = TimeSpan.FromMilliseconds(1);
            MoveAlongWaypointPath(new TimeInterval(oneMillisecond, oneMillisecond));
        }
    }

    // Port note: this is like 95% the same as CameraModFinalLookToward, we should probably merge them
    internal void CameraModLookToward(Vector3 loc)
    {
        if (DoingRotateCamera)
        {
            return;
        }

        if (DoingMoveCameraOnWaypointPath)
        {
            for (var i = 2; i <= _path.NumWaypoints; i++)
            {
                var result = _path.GetBezierPoint(i, 0.5f);

                var dir = new Vector2(
                    loc.X - result.X,
                    loc.Y - result.Y
                );

                var dirLength = dir.Length();
                if (dirLength < 0.1f)
                {
                    continue;
                }

                var angle = MathF.Acos(dir.X / dirLength);
                if (dir.Y < 0)
                {
                    angle = -angle;
                }

                // "Default camera is rotated 90 degrees, so match" says the original code
                angle -= MathUtility.PiOver2;
                angle = NormalizeAngle(angle);
                _path.CameraAngle[i] = angle;
            }

            if (_path.TotalTimeMilliseconds == 1)
            {
                // Do it instantly
                var oneMillisecond = TimeSpan.FromMilliseconds(1);
                MoveAlongWaypointPath(new TimeInterval(oneMillisecond, oneMillisecond));
                _cameraArrivedAtWaypointOnPathFlag = false;
            }
        }
    }

    internal void CameraModFinalLookToward(Vector3 loc)
    {
        if (DoingRotateCamera)
        {
            return;
        }

        if (DoingMoveCameraOnWaypointPath)
        {
            var min = _path.NumWaypoints - 1;
            if (min < 2)
            {
                min = 2;
            }

            for (var i = min; i <= _path.NumWaypoints; i++)
            {
                var result = _path.GetBezierPoint(i, 0.5f);

                var dir = new Vector2(
                    loc.X - result.X,
                    loc.Y - result.Y
                );

                var dirLength = dir.Length();
                if (dirLength < 0.1f)
                {
                    continue;
                }

                var angle = MathF.Acos(dir.X / dirLength);
                if (dir.Y < 0)
                {
                    angle = -angle;
                }

                // "Default camera is rotated 90 degrees, so match" says the original code
                angle -= MathUtility.PiOver2;
                angle = NormalizeAngle(angle);

                if (i == _path.NumWaypoints)
                {
                    _path.CameraAngle[i] = angle;
                }
                else
                {
                    var deltaAngle = NormalizeAngle(angle - _path.CameraAngle[i]);
                    angle = NormalizeAngle(_path.CameraAngle[i] + deltaAngle / 2.0f);
                    _path.CameraAngle[i] = angle;
                }
            }
        }
    }

    internal void CameraModFinalPitch(float finalPitch, float easeIn, float easeOut)
    {
        if (DoingRotateCamera)
        {
            var time = _rotateAnimation!.Duration + _rotateAnimation.HoldTime - _rotateAnimation.CurrentTime;
            PitchCamera(finalPitch, time, (float)(time * easeIn), (float)(time * easeOut));
        }

        if (DoingMoveCameraOnWaypointPath)
        {
            var time = _path!.TotalTimeMilliseconds - _path.ElapsedTimeMilliseconds;
            PitchCamera(finalPitch, time, (float)(time * easeIn), (float)(time * easeOut));
        }
    }

    internal void ZoomCamera(float finalZoom, double milliseconds, float easeIn, float easeOut)
    {
        // Generals first makes sure milliseconds is at least 1,
        // and then it converts it to number of logic frames, with a minimum of 1 frame (33.3333 ms).
        var duration = Math.Max(milliseconds, 1000.0 / 30.0);
        _zoomAnimation = new ZoomCameraInfo
        {
            Duration = duration,
            StartZoom = _zoom,
            EndZoom = finalZoom,
        };
        _zoomAnimation.Ease.SetEaseTimes((float)(easeIn / milliseconds), (float)(easeOut / milliseconds));
    }

    internal void PitchCamera(float finalPitch, double milliseconds, float easeIn, float easeOut)
    {
        var duration = Math.Max(milliseconds, 1000.0 / 30.0);
        _pitchAnimation = new PitchCameraInfo
        {
            Duration = duration,
            StartPitch = _fxPitch,
            EndPitch = finalPitch,
        };
        _pitchAnimation.Ease.SetEaseTimes((float)(easeIn / milliseconds), (float)(easeOut / milliseconds));
    }

    internal void CameraModFinalZoom(float finalZoom, float easeIn, float easeOut)
    {
        if (DoingRotateCamera)
        {
            var terrainHeightMax = GetHeightAroundPos(_pos.Vector2XY());
            var maxHeight = terrainHeightMax + _maxHeightAboveGround;
            var maxZoom = maxHeight / _cameraOffset.Z;
            // Generals converts here to ms, but our values are already in ms
            var time = _rotateAnimation!.Duration + _rotateAnimation!.HoldTime - _rotateAnimation.CurrentTime;
            ZoomCamera(finalZoom * maxZoom, time, (float)(time * easeIn), (float)(time * easeOut));
        }

        if (DoingMoveCameraOnWaypointPath)
        {
            var pos = _path!.Waypoints[_path.NumWaypoints];
            var terrainHeightMax = GetHeightAroundPos(pos.Vector2XY());
            var maxHeight = terrainHeightMax + _maxHeightAboveGround;
            var maxZoom = maxHeight / _cameraOffset.Z;
            var time = _path.TotalTimeMilliseconds - _path.ElapsedTimeMilliseconds;
            ZoomCamera(finalZoom * maxZoom, time, (float)(time * easeIn), (float)(time * easeOut));
        }
    }

    internal void RotateCamera(float rotations, double milliseconds, float easeIn, float easeOut)
    {
        milliseconds = Math.Max(milliseconds, 1);
        var duration = Math.Max(milliseconds, 1000.0 / 30.0);
        _rotateAnimation = new RotateCameraInfo
        {
            HoldTime = 0,
            Duration = duration,
            Angle = new RotateCameraInfoAngle
            {
                Start = _angle,
                End = _angle + rotations * MathUtility.TwoPi
            },
            StartTimeMultiplier = TimeMultiplier,
            EndTimeMultiplier = TimeMultiplier,
            Ease = new ParabolicEase((float)(easeIn / milliseconds), (float)(easeOut / milliseconds)),
        };

        _path = null;
        _cameraArrivedAtWaypointOnPathFlag = false;
    }

    internal void RotateCameraTowardObject(GameObject obj, double milliseconds, double holdMilliseconds, float easeIn, float easeOut)
    {
        var duration = Math.Max(milliseconds, 1000.0 / 30.0);
        _rotateAnimation = new RotateCameraInfo
        {
            HoldTime = Math.Max(holdMilliseconds, 0),
            Duration = duration,
            Target = new RotateCameraInfoTarget
            {
                Object = obj,
                Position = obj.Translation
            },
            StartTimeMultiplier = TimeMultiplier,
            EndTimeMultiplier = TimeMultiplier,
            Ease = new ParabolicEase((float)(easeIn / milliseconds), (float)(easeOut / milliseconds)),
            TrackObject = true
        };

        _path = null;
        _cameraArrivedAtWaypointOnPathFlag = false;
    }

    internal void RotateCameraTowardPosition(Vector3 pos, double milliseconds, float easeIn, float easeOut, bool reverseRotation)
    {
        var duration = Math.Max(milliseconds, 1000.0 / 30.0);

        var curPos = _pos;
        var dir = pos - curPos;
        var dirLength = dir.Length();

        if (dirLength < 0.1f)
        {
            return;
        }

        var angle = MathF.Acos(dir.X / dirLength);

        if (dir.Y < 0)
        {
            angle = -angle;
        }

        // Default camera is rotated 90 degrees, so match
        angle -= MathUtility.PiOver2;
        angle = NormalizeAngle(angle);

        if (reverseRotation)
        {
            if (_angle < angle)
            {
                angle -= MathUtility.TwoPi;
            }
            else
            {
                angle += MathUtility.TwoPi;
            }
        }

        _rotateAnimation = new RotateCameraInfo
        {
            HoldTime = 0,
            Duration = duration,
            Angle = new RotateCameraInfoAngle
            {
                Start = _angle,
                End = angle
            },
            StartTimeMultiplier = TimeMultiplier,
            EndTimeMultiplier = TimeMultiplier,
            Ease = new ParabolicEase((float)(easeIn / milliseconds), (float)(easeOut / milliseconds)),
            TrackObject = false
        };

        _path = null;
        _cameraArrivedAtWaypointOnPathFlag = false;
    }

    internal void ResetCamera(Vector3 location, double milliseconds, float easeIn, float easeOut)
    {
        MoveCameraTo(location, milliseconds, 0, false, easeIn, easeOut);
        _path.CameraAngle[2] = 0.0f;
        _angle = _path.CameraAngle[0];

        var terrainHeightMax = GetHeightAroundPos(location.Vector2XY());
        var desiredHeight = terrainHeightMax + _maxHeightAboveGround;
        var desiredZoom = desiredHeight / _cameraOffset.Z;

        ZoomCamera(desiredZoom, milliseconds, easeIn, easeOut);

        PitchCamera(1.0f, milliseconds, easeIn, easeOut);
    }

    internal void SetSnapMode(LockType lockType, float lockDist)
    {
        _lockType = lockType;
        _lockDist = lockDist;
        _doingScriptedCameraLock = true;
    }

    internal void CameraModFreezeAngle()
    {
        if (DoingRotateCamera)
        {
            if (_rotateAnimation.TrackObject && _rotateAnimation.Target != null)
            {
                _rotateAnimation.Target.Object = null;
            }
            else if (_rotateAnimation.Angle != null)
            {
                _rotateAnimation.Angle.Start = _rotateAnimation.Angle.End + _angle;
            }
        }

        if (DoingMoveCameraOnWaypointPath)
        {
            for (var i = 0; i < _path.NumWaypoints; i++)
            {
                // This actually goes past NumWaypoints because of the +1, but that's fine because the array is always at least 2 larger than NumWaypoints
                _path.CameraAngle[i + 1] = _path.CameraAngle[0];
            }
        }
    }

    internal void CameraModRollingAverage(int framesToAverage)
    {
        framesToAverage = Math.Max(1, framesToAverage);
        if (DoingMoveCameraOnWaypointPath)
        {
            _path.RollingAverageFrames = framesToAverage;
        }
    }

    internal void CameraModFinalTimeMultiplier(int finalMultiplier)
    {
        if (DoingZoomCamera)
        {
            _zoomAnimation.EndTimeMultiplier = finalMultiplier;
        }

        if (DoingPitchCamera)
        {
            _pitchAnimation.EndTimeMultiplier = finalMultiplier;
        }

        if (DoingRotateCamera)
        {
            _rotateAnimation.EndTimeMultiplier = finalMultiplier;
        }
        else if (DoingMoveCameraOnWaypointPath)
        {
            var curDistance = 0.0f;

            for (var i = 0; i < _path.NumWaypoints; i++)
            {
                curDistance += _path.WaySegLength[i];
                var factor2 = curDistance / _path.TotalDistance;
                var factor1 = 1.0f - factor2;
                _path.TimeMultiplier[i + 1] = (int)MathF.Floor(0.5f + _path.TimeMultiplier[i + 1] * factor1 + finalMultiplier * factor2);
            }
        }
        else
        {
            TimeMultiplier = finalMultiplier;
        }
    }

    internal void CameraModFinalMoveTo(Vector3 pos)
    {
        if (DoingRotateCamera)
        {
            return;
        }

        if (DoingMoveCameraOnWaypointPath)
        {
            var start = _path.Waypoints[_path.NumWaypoints];
            var delta = pos - start;
            for (var i = 2; i <= _path.NumWaypoints; i++)
            {
                _path.Waypoints[i] += delta;
            }
        }
    }

    internal void MoveCameraAlongWaypointPath(Waypoint? waypoint, double milliseconds, int shutter, bool orient, float easeIn, float easeOut)
    {
        if (waypoint == null)
        {
            throw new ArgumentNullException(nameof(waypoint), "Waypoint path must have at least one waypoint.");
        }

        const float MinDelta = HeightMap.HorizontalScale;
        var path = new MoveAlongWaypointPathInfo();

        path.Waypoints[0] = waypoint.Position;
        path.CameraAngle[0] = _angle;
        path.WaySegLength[0] = 0;
        path.Waypoints[1] = waypoint.Position;
        path.NumWaypoints = 1;
        // Ensure this movement takes at least 1 millisecond, to avoid division by zero.
        milliseconds = Math.Max(milliseconds, 1);
        path.TotalTimeMilliseconds = milliseconds;
        path.Ease.SetEaseTimes((float)(easeIn / milliseconds), (float)(easeOut / milliseconds));

        // The shutter / stutter value is used to add intentional stutter to the camera movement (which looks ugly but it was the early 2000s).
        // When shutter is 0 (or less than delta time), the camera position is updated every frame.
        // When shutter is set to a value greater than 0, the camera position is updated every shutter milliseconds.
        // Shutter has no effect on the time it takes to complete the movement, only how often the camera position visibly updates.

        // The original code, like everything else in Generals, was designed for fixed 30 FPS time step.
        // The shutter argument was in milliseconds (as it is here), but the Shutter field on the path struct was in frames
        // To support arbitrary frame rates we'll store shutter as (float) milliseconds.
        // Generals has a minimum shutter of 1 frame, but whether it's 0 or 1 doesn't really matter at all, because CurShutter was
        // unconditionally decremented by 1 every frame, before the early return condition.
        // We can just set the minimum shutter to 0.0f to ensure smooth movement when running at high frame rates.
        path.Shutter = Math.Max(shutter, 0.0f);

        // Iterate through the waypoints and add them to the path.
        while (waypoint != null && path.NumWaypoints < MoveAlongWaypointPathInfo.MaxWaypoints)
        {
            path.NumWaypoints++;
            path.Waypoints[path.NumWaypoints] = waypoint.Position;

            if (waypoint.ConnectedWaypoints.Count > 0)
            {
                waypoint = waypoint.ConnectedWaypoints[0];
            }
            else
            {
                waypoint = null;
            }

            var dir = path.Waypoints[path.NumWaypoints].Vector2XY() - path.Waypoints[path.NumWaypoints - 1].Vector2XY();
            // Check if this waypoint is too close to the previous one
            if (dir.Length() < MinDelta)
            {
                if (waypoint != null)
                {
                    // If this was the last waypoint, just drop it
                    path.NumWaypoints--;
                }
                else
                {
                    // If not, replace the previous waypoint in the path with this one
                    path.Waypoints[path.NumWaypoints - 1] = path.Waypoints[path.NumWaypoints];
                    path.NumWaypoints--;
                }
            }
        }

        SetupWaypointPath(path, orient);
    }

    // Port note: in C++ this function doesn't receive the path as a parameter and instead reads it from a field
    // However I think it makes sense to finish setting up the path before it's assigned to the field

    void SetupWaypointPath(MoveAlongWaypointPathInfo path, bool orient)
    {
        path.CurSegment = 1;
        path.CurSegDistance = 0;
        path.TotalDistance = 0;
        path.RollingAverageFrames = 1;

        var angle = _angle;

        for (var i = 1; i < path.NumWaypoints; i++)
        {
            var dir = path.Waypoints[i + 1].Vector2XY() - path.Waypoints[i].Vector2XY();
            path.WaySegLength[i] = dir.Length();
            path.TotalDistance += path.WaySegLength[i];
            if (orient)
            {
                angle = MathF.Acos(dir.X / path.WaySegLength[i]);

                if (dir.Y < 0)
                {
                    angle = -angle;
                }

                // Default camera is rotated 90 degrees, so match
                angle -= MathUtility.PiOver2;
                angle = NormalizeAngle(angle);
            }

            path.CameraAngle[i] = angle;
        }

        path.CameraAngle[1] = _angle;
        path.CameraAngle[path.NumWaypoints] = path.CameraAngle[path.NumWaypoints - 1];

        for (var i = path.NumWaypoints - 1; i > 1; i--)
        {
            path.CameraAngle[i] = (path.CameraAngle[i] + path.CameraAngle[i - 1]) / 2;
        }

        path.WaySegLength[path.NumWaypoints + 1] = path.WaySegLength[path.NumWaypoints];

        // Prevent a possible divide by zero
        if (path.TotalDistance < 1.0)
        {
            path.WaySegLength[path.NumWaypoints - 1] += 1.0f - path.TotalDistance;
            path.TotalDistance = 1.0f;
        }

        var curDistance = 0.0f;
        var finalPos = path.Waypoints[path.NumWaypoints];
        var newGround = _terrainLogic.GetGroundHeight(finalPos.X, finalPos.Y);

        for (var i = 0; i <= path.NumWaypoints + 1; i++)
        {
            var factor2 = curDistance / path.TotalDistance;
            var factor1 = 1.0f - factor2;
            path.TimeMultiplier[i] = TimeMultiplier;
            path.GroundHeight[i] = _groundLevel * factor1 + newGround * factor2;
            curDistance += path.WaySegLength[i];
        }

        // Pad the end
        path.Waypoints[path.NumWaypoints + 1] = path.Waypoints[path.NumWaypoints];
        var cur = path.Waypoints[path.NumWaypoints];
        var prev = path.Waypoints[path.NumWaypoints - 1];
        path.Waypoints[path.NumWaypoints + 1] += cur - prev;
        path.CameraAngle[path.NumWaypoints + 1] = path.CameraAngle[path.NumWaypoints];
        path.GroundHeight[path.NumWaypoints + 1] = newGround;

        cur = path.Waypoints[2];
        prev = path.Waypoints[1];
        path.Waypoints[0] -= cur - prev;

        if (path.NumWaypoints > 1)
        {
            _path = path;
        }
        _cameraArrivedAtWaypointOnPathFlag = false;
        _rotateAnimation = null;
        path.ElapsedTimeMilliseconds = 0;
        path.CurShutter = path.Shutter;
    }

    void MoveAlongWaypointPath(in TimeInterval gameTime)
    {
        // TODO(Port): Add equivalent to TheGlobalData->m_disableCameraMovement?

        if (!DoingMoveCameraOnWaypointPath)
        {
            // This should be unreachable, but just in case
            return;
        }

        _path.ElapsedTimeMilliseconds += gameTime.DeltaTime.TotalMilliseconds;

        if (_path.ElapsedTimeMilliseconds > _path.TotalTimeMilliseconds)
        {
            // We've reached the end of the path
            _cameraArrivedAtWaypointOnPathFlag = false;
            // TODO(Port)
            //m_freezeTimeForCameraMovement = false;
            _angle = _path.CameraAngle[_path.NumWaypoints];
            _groundLevel = _path.GroundHeight[_path.NumWaypoints];
            UpdateCameraOffsetBasedOnZ();
            var pos = _path.Waypoints[_path.NumWaypoints];
            _path = null;
            pos.Z = 0.0f;
            _pos = pos;
            ExpandConstraintsToContain(pos);
            return;
        }

        var totalTime = _path.TotalTimeMilliseconds;
        var deltaTime = _path.Ease.Evaluate((float)(_path.ElapsedTimeMilliseconds / totalTime)) -
                        _path.Ease.Evaluate((float)((_path.ElapsedTimeMilliseconds - gameTime.DeltaTime.TotalMilliseconds) / totalTime));
        _path.CurSegDistance += deltaTime * _path.TotalDistance;

        while (_path.CurSegDistance >= _path.WaySegLength[_path.CurSegment])
        {
            // I don't see how this could ever be false, but the original code has a check for it
            if (DoingMoveCameraOnWaypointPath)
            {
                _cameraArrivedAtWaypointOnPathFlag = true;
            }

            _path.CurSegDistance -= _path.WaySegLength[_path.CurSegment];
            _path.CurSegment++;

            if (_path.CurSegment >= _path.NumWaypoints)
            {
                // Set total time to zero to end the path on the next frame
                _path.TotalTimeMilliseconds = 0;
                return;
            }
        }

        var avgFactor = 1.0f / _path.RollingAverageFrames;
        _path.CurShutter -= gameTime.DeltaTime.TotalMilliseconds;

        // If current shutter is over zero, we wait
        if (_path.CurShutter > 0)
        {
            return;
        }

        // Reset shutter to the path's shutter value
        _path.CurShutter = _path.Shutter;

        var factor = _path.CurSegDistance / _path.WaySegLength[_path.CurSegment];

        if (_path.CurSegment == _path.NumWaypoints - 1)
        {
            avgFactor += (1.0f - avgFactor) * factor;
        }

        var factor1 = 1.0f - factor;
        var factor2 = 1.0f - factor1;

        var currentAngle = _path.CameraAngle[_path.CurSegment];
        var nextAngle = _path.CameraAngle[_path.CurSegment + 1];
        if (nextAngle - currentAngle > MathF.PI)
        {
            currentAngle += MathUtility.TwoPi;
        }
        if (nextAngle - currentAngle < -Math.PI)
        {
            currentAngle -= MathUtility.TwoPi;
        }

        var angle = NormalizeAngle(currentAngle * factor1 + nextAngle * factor2);
        var deltaAngle = NormalizeAngle(angle - _angle);
        _angle += avgFactor * deltaAngle;
        _angle = NormalizeAngle(_angle);

        var timeMultiplier = _path.TimeMultiplier[_path.CurSegment] * factor1 +
                             _path.TimeMultiplier[_path.CurSegment + 1] * factor2;
        TimeMultiplier = (int)MathF.Floor(0.5f + timeMultiplier);

        _groundLevel = _path.GroundHeight[_path.CurSegment] * factor1 +
                       _path.GroundHeight[_path.CurSegment + 1] * factor2;
        UpdateCameraOffsetBasedOnZ();

        int midSegmentIndex;
        if (factor < 0.5)
        {
            midSegmentIndex = _path.CurSegment;
            factor += 0.5f;
        }
        else
        {
            midSegmentIndex = _path.CurSegment + 1;
            factor -= 0.5f;
        }
        var result = _path.GetBezierPoint(midSegmentIndex, factor);

        _pos = result;
        ExpandConstraintsToContain(result);
    }

    private void ExpandConstraintsToContain(Vector3 pos)
    {
        _cameraConstraint = RectangleF.FromMinMax(
            new Vector2(
                MathF.Min(_cameraConstraint.Left, pos.X),
                MathF.Min(_cameraConstraint.Top, pos.Y)
            ),
            new Vector2(
                MathF.Max(_cameraConstraint.Right, pos.X),
                MathF.Max(_cameraConstraint.Bottom, pos.Y)
            )
        );
    }

    private bool UpdateCameraMovements(in TimeInterval gameTime)
    {
        var didUpdate = false;
        if (DoingZoomCamera)
        {
            ZoomCameraOneFrame(gameTime);
            didUpdate = true;
        }

        if (DoingPitchCamera)
        {
            PitchCameraOneFrame(gameTime);
            didUpdate = true;
        }

        if (DoingRotateCamera)
        {
            _previousLookAtPosition = _pos;
            RotateCameraOneFrame(gameTime);
            didUpdate = true;
        }

        if (DoingMoveCameraOnWaypointPath)
        {
            _previousLookAtPosition = _pos;
            MoveAlongWaypointPath(gameTime);
            didUpdate = true;
        }

        if (_doingScriptedCameraLock)
        {
            didUpdate = true;
        }

        return didUpdate;
    }

    // In C++ this is a static variable inside W3DView::update
    private float _followFactor = -1.0f;

    public void Update(in TimeInterval gameTime)
    {
        LookAtTranslator.Update(gameTime);

        // TODO(Port): Is any of this relevant?
        // if (TheTerrainRenderObject->doesNeedFullUpdate())
        // {
        //     RefRenderObjListIterator *it = W3DDisplay::m_3DScene->createLightsIterator();
        //     TheTerrainRenderObject->updateCenter(m_3DCamera, it);
        //     if (it)
        //     {
        //         W3DDisplay::m_3DScene->destroyLightsIterator(it);
        //         it = NULL;
        //     }
        // }

        HandleObjectLock(gameTime, out var didScriptedMovement, out var recalcCamera);

        // TODO(Port): More checks
        // 	if (!(TheScriptEngine->isTimeFrozenDebug() /* || TheScriptEngine->isTimeFrozenScript()*/) && !TheGameLogic->isGamePaused())
        var isTimeFrozenOrPaused = false;

        if (!isTimeFrozenOrPaused)
        {
            if (UpdateCameraMovements(gameTime))
            {
                didScriptedMovement = true;
                recalcCamera = true;
            }
        }
        else
        {
            if (DoingRotateCamera || DoingMoveCameraOnWaypointPath || DoingPitchCamera || DoingZoomCamera || _doingScriptedCameraLock)
            {
                didScriptedMovement = true;
            }
        }

        // TODO(Port): Camera shake
        // if (m_shakeIntensity > 0.01f)
        // {
        //     m_shakeOffset.x = m_shakeIntensity * m_shakeAngleCos;
        //     m_shakeOffset.y = m_shakeIntensity * m_shakeAngleSin;
        //
        //     // fake a stiff spring/damper
        //     const Real dampingCoeff = 0.75f;
        //     m_shakeIntensity *= dampingCoeff;
        //
        //     // spring is so "stiff", it pulls 180 degrees opposite each frame
        //     m_shakeAngleCos = -m_shakeAngleCos;
        //     m_shakeAngleSin = -m_shakeAngleSin;
        //
        //     recalcCamera = true;
        // }
        // else
        // {
        //     m_shakeIntensity = 0.0f;
        //     m_shakeOffset.x = 0.0f;
        //     m_shakeOffset.y = 0.0f;
        // }
        //
        // // Process New C3 Camera Shaker system
        // if (CameraShakerSystem.IsCameraShaking())
        // {
        //     recalcCamera = true;
        // }


        /*
         * In order to have the camera follow the terrain in a non-dizzying way, we will have a
         * "desired height" value that the user sets.  While scrolling, the actual height (set by
         * zoom) will not get updated unless we are scrolling uphill and our view either goes
         * underground or higher than the max allowed height.  When the camera is at rest (not
         * scrolling), the zoom will move toward matching the desired height.
         */

        // In Generals these two are fields, but their only non-local uses seem to be for debugging
        // This variable is actually named a bit misleadingly: _pos is the target position, not the camera position
        var terrainHeightUnderCamera = GetHeightAroundPos(_pos.Vector2XY());
        var currentHeightAboveGround = _cameraOffset.Z * _zoom - terrainHeightUnderCamera;

        // In Generals this ia flag with public set/get methods, but looks like it's basically always true?
        var okToAdjustHeight = true;
        // TODO(Port): Replace with actual check
        var isGamePaused = false;

        if (okToAdjustHeight && !isGamePaused)
        {
            var desiredHeight = terrainHeightUnderCamera + _heightAboveGround;
            var desiredZoom = desiredHeight / _cameraOffset.Z;

            // TODO(Port): Update this
            var isInReplayGame = false;
            // This should be controlled by the matching game setting
            var useCameraInReplay = _gameData.UseCameraInReplay;
            if (didScriptedMovement || (isInReplayGame && useCameraInReplay))
            {
                // If we are in a scripted camera movement, take its height above ground as our desired height
                _heightAboveGround = currentHeightAboveGround;
            }

            if (LookAtTranslator.IsScrolling)
            {
                // If scrolling, only adjust if we're too close or too far
                if (
                    _scrollAmount.Length() < _scrollAmountCutoff ||
                    (currentHeightAboveGround < _minHeightAboveGround) ||
                    (_gameData.EnforceMaxCameraHeight && currentHeightAboveGround > _maxHeightAboveGround))
                {
                    var zoomAdj = (desiredZoom - _zoom) * _gameData.CameraAdjustSpeed;
                    if (MathF.Abs(zoomAdj) >= 0.0001)
                    {
                        _zoom += (float)(zoomAdj * gameTime.LogicFrameRelativeDeltaTime);
                        recalcCamera = true;
                    }
                }
            }
            else
            {
                // We're not scrolling; settle toward desired height above ground
                var zoomAdj = (_zoom - desiredZoom) * _gameData.CameraAdjustSpeed;
                var zoomAdjAbs = MathF.Abs(zoomAdj);
                if (zoomAdjAbs >= 0.0001 && !didScriptedMovement)
                {
                    _zoom -= (float)(zoomAdj * gameTime.LogicFrameRelativeDeltaTime);
                    recalcCamera = true;
                }
            }
        }

        // TODO(Port)
        // TheScriptEngine->isTimeFast()
        var isTimeFast = false;
        if (isTimeFast)
        {
            return;
        }

        if (recalcCamera)
        {
            SetCameraTransform();
        }

        // C++ does culling here, but we don't need to do it in the camera controller
    }

    // This was split from W3DView::update
    private void HandleObjectLock(in TimeInterval gameTime, out bool didScriptedMovement, out bool recalcCamera)
    {
        if (CameraLock == null)
        {
            _followFactor = -1.0f;
            didScriptedMovement = false;
            recalcCamera = false;
            return;
        }

        // If we have a camera lock, stop current path animation
        _path = null;
        _cameraArrivedAtWaypointOnPathFlag = false;

        //  ... but we only check afterwards if the object we are following is still alive

        // TODO: Remove lock when the object has been destroyed
        // C++ version only stores an object ID and fetches the object from the scene,
        // which handles that automatically
        if (CameraLock.IsDead)
        {
            CameraLock = null;
            _followFactor = -1.0f;
            didScriptedMovement = false;
            recalcCamera = false;
            return;
        }

        if (_followFactor < 0)
        {
            _followFactor = 0.05f;
        }
        else
        {
            // In C++ this is a fixed addition, but that won't work at arbitrary frame rates
            _followFactor += (float)(0.05 * gameTime.LogicFrameRelativeDeltaTime);
            _followFactor = Math.Min(_followFactor, 1.0f);
        }

        // TODO: Can Drawables be null? GameObject is not yet null-checked
        if (CameraLock.Drawable != null)
        {
            if (CameraLock.Drawable.GetFirstRenderObjInfo(out var boundingSphereRadius, out var transform))
            {
                boundingSphereRadius = 1.0f;
                var objPos = transform.Translation;
                // Get position of top of object, assuming world z roughly along local z
                objPos += Vector3.UnitZ * boundingSphereRadius;
                var objView = transform.Matrix.GetXVector();
                // Move camera back behind object far enough not to intersect bounding sphere
                var camTran = objPos - objView * boundingSphereRadius * 4.5f;
                // Importantly, this is the actual camera position, not the target position (like _pos)
                var prevCamTran = Camera.Position;
                var tranDiff = camTran - prevCamTran;
                // Slowly move camera to new position
                camTran = prevCamTran + tranDiff * 0.1f;
                Camera.SetLookAt(camTran, objPos, Vector3.UnitZ);
                // No need to recalculate camera since we already did it
                recalcCamera = false;
                didScriptedMovement = false;
                return;
            }

            // This object has a drawable but for some reason we didn't render bounding sphere / transform from it
            // Generals does nothing here, so we'll do the same

            didScriptedMovement = false;
            recalcCamera = false;
            return;
        }
        else
        {
            // TODO: This probably doesn't handle over 30 FPS correctly

            // We have a camera lock, but the object doesn't have a drawable
            var objPos = CameraLock.Transform.Translation;
            var curPos = _pos;

            var snapThreshSqr = MathF.Sqrt(_gameData.PartitionCellSize);
            var curDistSqr = MathF.Sqrt(curPos.X - objPos.X) + MathF.Sqrt(curPos.Y - objPos.Y);

            if (_snapImmediate)
            {
                curPos = objPos;
            }
            else
            {
                var d = objPos - curPos;

                if (_lockType == LockType.Tether)
                {
                    if (curDistSqr >= snapThreshSqr)
                    {
                        var ratio = 1.0f - snapThreshSqr / curDistSqr;
                        // Move halfway there
                        curPos += d * ratio * 0.5f;
                    }
                    else
                    {
                        // We're inside our 'play' tolerance.  Move slowly to the obj
                        var ratio = 0.01f * _lockDist;
                        var dInner = objPos - curPos;
                        curPos += dInner * ratio;
                    }
                }
                else
                {
                    curPos += d * _followFactor;
                }
            }

            // TODO(Port): Add these checks
            // if (!(TheScriptEngine->isTimeFrozenDebug() || TheScriptEngine->isTimeFrozenScript()) && !TheGameLogic->isGamePaused())
            // {
            //     m_previousLookAtPosition = *getPosition();
            // }

            _pos = curPos;

            if (_lockType == LockType.Follow)
            {
                // Camera follow objects if they are flying

                if (CameraLock.IsUsingAirborneLocomotor() && CameraLock.IsAboveTerrainOrWater)
                {
                    var oldZRot = NormalizeAngle(_angle);
                    // TODO: This might be incorrect; in C++ it uses Thing::getOrientation()
                    var idealZRot = NormalizeAngle(CameraLock.Transform.Yaw - MathUtility.PiOver2);
                    var diff = NormalizeAngle(idealZRot - oldZRot);

                    if (_snapImmediate)
                    {
                        _angle = idealZRot;
                    }
                    else
                    {
                        // 30 FPS fix, might need to be adjusted
                        _angle += (float)(diff * 0.1 * gameTime.LogicFrameRelativeDeltaTime);
                    }
                    _angle = NormalizeAngle(_angle);
                }
            }

            if (_snapImmediate)
            {
                _snapImmediate = false;
            }

            _groundLevel = objPos.Z;
            didScriptedMovement = true;
            recalcCamera = true;
        }
    }

    private void SetCameraTransform()
    {
        // m_cameraHasMovedSinceRequest = true;

        if (!_cameraConstraintValid)
        {
            BuildCameraTransform();
            CalcCameraConstraints();
        }

        if (_cameraConstraintValid)
        {
            _pos = ClampPositionToConstraints(_pos);
        }

        BuildCameraTransform();
    }

    private Vector3 ClampPositionToConstraints(Vector3 pos)
    {
        // This deviates from the original, but the original code crashed Math.Clamp
        // because sometimes min > max

        var minX = Math.Min(_cameraConstraint.Left, _cameraConstraint.Right);
        var minY = Math.Min(_cameraConstraint.Top, _cameraConstraint.Bottom);
        var maxX = Math.Max(_cameraConstraint.Left, _cameraConstraint.Right);
        var maxY = Math.Max(_cameraConstraint.Top, _cameraConstraint.Bottom);
        pos.X = Math.Clamp(pos.X, minX, maxX);
        pos.Y = Math.Clamp(pos.Y, minY, maxY);
        return pos;
    }

    private void CalcCameraConstraints()
    {
        var mapRegion = _terrainLogic.GetExtent();
        var maxEdgeZ = _groundLevel;

        var screenCenter = new Vector2(
            MathF.Floor(Camera.ScreenSize.X / 2),
            MathF.Floor(Camera.ScreenSize.Y / 2)
        );

        var (centerRayStart, centerRayEnd) = Camera.GetPickRay(screenCenter);

        var center = new Vector3(
            Vector3Utility.FindXAtZ(maxEdgeZ, centerRayStart, centerRayEnd),
            Vector3Utility.FindYAtZ(maxEdgeZ, centerRayStart, centerRayEnd),
            maxEdgeZ
        );

        var screenBottom = new Vector2(
            screenCenter.X,
            MathF.Floor(Camera.ScreenSize.Y * 0.95f)
        );
        var (bottomRayStart, bottomRayEnd) = Camera.GetPickRay(screenBottom);

        var bottom = new Vector3(
            Vector3Utility.FindXAtZ(maxEdgeZ, bottomRayStart, bottomRayEnd),
            Vector3Utility.FindYAtZ(maxEdgeZ, bottomRayStart, bottomRayEnd),
            maxEdgeZ
        );

        center.X -= bottom.X;
        center.Y -= bottom.Y;

        var offset = center.Length();
        var topLeft = new Vector2(
            mapRegion.Left + offset,
            mapRegion.Back + offset
        );
        var bottomRight = new Vector2(
            mapRegion.Right - offset,
            mapRegion.Front - offset
        );
        _cameraConstraint = new RectangleF(
            topLeft.X,
            topLeft.Y,
            bottomRight.X - topLeft.X,
            bottomRight.Y - topLeft.Y
        );
        _cameraConstraintValid = true;
    }

    private void BuildCameraTransform()
    {
        var groundLevel = _groundLevel;

        var zoom = _zoom;
        var angle = _angle;
        var pitch = _pitchAngle;
        var pos = _pos;

        // TODO(Port)
        // add in the camera shake, if any
        // pos.x += m_shakeOffset.x;
        // pos.y += m_shakeOffset.y;

        if (_cameraConstraintValid)
        {
            pos = ClampPositionToConstraints(pos);
        }

        // Port note: in C++ the camera supports something called "real zoom" - however its never enabled in the code
        // So we're skipping it for now

        // Set position of camera itself
        var sourcePos = _cameraOffset * zoom;

        var factor = 1.0f - (groundLevel / sourcePos.Z);

        var angleTransform = Matrix4x4.CreateRotationZ(angle);
        var pitchTransform = Matrix4x4.CreateRotationX(pitch);
        sourcePos = Vector3.Transform(sourcePos, pitchTransform);
        sourcePos = Vector3.Transform(sourcePos, angleTransform);
        sourcePos *= factor;

        sourcePos += new Vector3(pos.X, pos.Y, groundLevel);
        var targetPos = new Vector3(pos.X, pos.Y, groundLevel);

        if (_fxPitch <= 1.0f)
        {
            var height = sourcePos.Z - targetPos.Z;
            height *= _fxPitch;
            targetPos.Z = sourcePos.Z - height;
        }
        else
        {
            sourcePos = targetPos + ((sourcePos - targetPos) / _fxPitch);
        }

        // Copy these to fields so that we can show them in the inspector
        _sourcePos = sourcePos;
        _targetPos = targetPos;

        // C++ version has this method return a matrix via a reference parameter,
        // but since it's always used to set the camera transform, we can just do that directly
        Camera.SetLookAt(sourcePos, targetPos, Vector3.UnitZ);

        // TODO(Port):
        // CameraShakerSystem.Timestep(1.0f / 30.0f);
        // CameraShakerSystem.Update_Camera_Shaker(sourcePos, &m_shakerAngles);
        // transform->Rotate_X(m_shakerAngles.X);
        // transform->Rotate_Y(m_shakerAngles.Y);
        // transform->Rotate_Z(m_shakerAngles.Z);

        // C++ checks for m_isCameraSlaved
        // From what I can tell that is related to 3DS Max integration, which we don't need
    }

    // This could be a property
    public void SetHeightAboveGround(float z)
    {
        _heightAboveGround = z;

        if (IsZoomLimited)
        {
            _heightAboveGround = Math.Clamp(_heightAboveGround, _minHeightAboveGround, _maxHeightAboveGround);
        }

        ResetAnimationsAndFlags();
        SetCameraTransform();
    }

    // C++: View::zoomIn
    public void ZoomIn()
    {
        SetHeightAboveGround((float)(_heightAboveGround - 10.0f));
    }

    // C++: View::zoomOut
    public void ZoomOut()
    {
        SetHeightAboveGround((float)(_heightAboveGround + 10.0f));
    }

    private void ZoomCameraOneFrame(in TimeInterval gameTime)
    {
        if (!DoingZoomCamera)
        {
            return;
        }

        _zoomAnimation.CurrentTime += gameTime.DeltaTime.TotalMilliseconds;

        // TODO(Port): check TheGlobalData->m_disableCameraMovement

        if (_zoomAnimation.CurrentTime <= _zoomAnimation.Duration)
        {
            var relativeTime = _zoomAnimation.CurrentTime / _zoomAnimation.Duration;
            var factor = _zoomAnimation.Ease.Evaluate((float)relativeTime);
            _zoom = MathUtility.Lerp(_zoomAnimation.StartZoom, _zoomAnimation.EndZoom, factor);
        }

        if (_zoomAnimation.CurrentTime >= _zoomAnimation.Duration)
        {
            _zoom = _zoomAnimation.EndZoom;
            _zoomAnimation = null;
        }
    }

    private void PitchCameraOneFrame(in TimeInterval gameTime)
    {
        if (!DoingPitchCamera)
        {
            return;
        }

        _pitchAnimation.CurrentTime += gameTime.DeltaTime.TotalMilliseconds;

        // TODO(Port): check TheGlobalData->m_disableCameraMovement

        if (_pitchAnimation.CurrentTime <= _pitchAnimation.Duration)
        {
            // Not just holding; do the camera adjustment
            var factor = _pitchAnimation.Ease.Evaluate((float)(_pitchAnimation.CurrentTime / _pitchAnimation.Duration));
            _fxPitch = MathUtility.Lerp(_pitchAnimation.StartPitch, _pitchAnimation.EndPitch, factor);
        }

        if (_pitchAnimation.CurrentTime >= _pitchAnimation.Duration)
        {
            _fxPitch = _pitchAnimation.EndPitch;
            _pitchAnimation = null;
        }
    }

    private void RotateCameraOneFrame(in TimeInterval gameTime)
    {
        if (!DoingRotateCamera)
        {
            return;
        }

        _rotateAnimation.CurrentTime += gameTime.DeltaTime.TotalMilliseconds;

        // TODO(Port): check TheGlobalData->m_disableCameraMovement

        if (_rotateAnimation.TrackObject)
        {
            if (_rotateAnimation.CurrentTime <= _rotateAnimation.Duration + _rotateAnimation.HoldTime)
            {
                // TOOD: Check if object is still alive, and if not, use the last known position
                _rotateAnimation.Target!.Position = _rotateAnimation.Target.Object?.Translation ?? _rotateAnimation.Target.Position;

                var dir = _rotateAnimation.Target.Position - _pos;
                var dirLength = dir.Length();

                if (dirLength >= 0.1f)
                {
                    var angle = MathF.Acos(dir.X / dirLength);
                    if (dir.Y < 0.0f)
                    {
                        angle = -angle;
                    }

                    // Default camera is rotated 90 degrees
                    _angle = angle + MathUtility.PiOver2;
                    angle = NormalizeAngle(angle);

                    if (_rotateAnimation.CurrentTime <= _rotateAnimation.Duration)
                    {
                        var factor = _rotateAnimation.Ease.Evaluate((float)(_rotateAnimation.CurrentTime / _rotateAnimation.Duration));
                        var angleDiff = NormalizeAngle(angle - _angle);
                        angleDiff *= factor;
                        _angle += angleDiff;
                        _angle = NormalizeAngle(_angle);
                        TimeMultiplier =
                            _rotateAnimation.StartTimeMultiplier +
                            (int)MathF.Floor(0.5f + (_rotateAnimation.EndTimeMultiplier - _rotateAnimation.StartTimeMultiplier) * factor);
                    }
                    else
                    {
                        _angle = angle;
                    }
                }
            }
        }
        else if (_rotateAnimation.CurrentTime <= _rotateAnimation.Duration)
        {
            var factor = _rotateAnimation.Ease.Evaluate((float)(_rotateAnimation.CurrentTime / _rotateAnimation.Duration));
            _angle = MathUtility.Lerp(_rotateAnimation.Angle!.Start, _rotateAnimation.Angle.End, factor);
            _angle = NormalizeAngle(_angle);
            TimeMultiplier = _rotateAnimation.StartTimeMultiplier +
                              (int)MathF.Floor(0.5f + (_rotateAnimation.EndTimeMultiplier - _rotateAnimation.StartTimeMultiplier) * factor);
        }

        if (_rotateAnimation.CurrentTime >= _rotateAnimation.Duration + _rotateAnimation.HoldTime)
        {
            // TODO(Port): m_freezeTimeForCameraMovement = false;
            if (!_rotateAnimation.TrackObject)
            {
                _angle = _rotateAnimation.Angle!.End;
            }

            _rotateAnimation = null;
        }
    }

    public void ScrollBy(Vector2 delta, in TimeInterval gameTime)
    {
        if (delta.LengthSquared() == 0)
        {
            return;
        }

        const float scrollResolution = 250.0f;
        _scrollAmount = delta;

        var screenSize = Camera.ScreenSize;
        var start = screenSize;
        // In C++ there was an attempt to compensate for aspect ratio, but it's incorrect
        // Because of integer division, the aspect ratio is always 1
        var aspect = 1.0f;
        // If we want to fix this, we can do the following:
        // var aspect = screenSize.X / screenSize.Y;
        var end = new Vector2(
            start.X + delta.X * scrollResolution,
            start.Y + delta.Y * scrollResolution * aspect
        );

        // TODO(Port): Our ScreenToWorldPoint works differently from Generals' Device_To_World_Space
        // As a result the movement is much faster than in Generals, unless we scale it down
        var worldStart = Camera.ScreenToWorldPoint(new Vector3(start, 0.0f));
        var worldEnd = Camera.ScreenToWorldPoint(new Vector3(end, 0.0f));
        var world = worldEnd - worldStart;
        // ...which we do here
        world *= 0.1f;

        // Original camera movement was designed for fixed 30 FPS time step, so we'll have to scale it with delta time
        world *= (float)gameTime.LogicFrameRelativeDeltaTime;

        _pos.X += world.X;
        _pos.Y += world.Y;

        _rotateAnimation = null;
        SetCameraTransform();
    }

    public void GoToObject(Logic.Object.GameObject gameObject)
    {
        LookAt(gameObject.Translation);
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistSingle(ref _angle);
        reader.PersistVector3(ref _cameraOffset);
    }

    private bool _showHexInInspector;

    internal void DrawInspector()
    {
        // For some reason local functions do not support overloads

        static string FloatToHex(float value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value)).ToString("X8");
        }

        void LabelTextWithHex(string label, float value)
        {
            ImGui.LabelText(label, _showHexInInspector ? $"{value} (0x{FloatToHex(value)})" : value.ToString());
        }

        void LabelTextWithHexVector3(string label, Vector3 value)
        {
            var stringified = $"{value.X}, {value.Y}, {value.Z}";

            ImGui.LabelText(
                label,
                _showHexInInspector ?
                    $"{stringified} (0x{FloatToHex(value.X)}, 0x{FloatToHex(value.Y)}, 0x{FloatToHex(value.Z)})" :
                    stringified
            );
        }

        void LabelTextWithHexRectangleF(string label, RectangleF value)
        {
            var stringified = $"{value.Left}, {value.Top}, {value.Width}, {value.Height}";

            ImGui.LabelText(
                label,
                _showHexInInspector ?
                    $"{stringified} (0x{FloatToHex(value.Left)}, 0x{FloatToHex(value.Top)}, 0x{FloatToHex(value.Width)}, 0x{FloatToHex(value.Height)})" :
                    stringified
            );
        }

        ImGui.Checkbox("Show hex", ref _showHexInInspector);

        ImGui.SeparatorText("Camera configuration");
        LabelTextWithHex("Yaw", _angle);
        LabelTextWithHex("Pitch", _pitchAngle);
        LabelTextWithHex("Zoom", _zoom);
        LabelTextWithHexVector3("Offset", _cameraOffset);
        LabelTextWithHexVector3("Position", _pos);
        LabelTextWithHex("Height above ground", _heightAboveGround);

        ImGui.SeparatorText("Computed values");
        LabelTextWithHex("Ground level", _groundLevel);
        LabelTextWithHexVector3("Source position", _sourcePos);
        LabelTextWithHexVector3("Target position", _targetPos);
        LabelTextWithHexRectangleF("Camera constraint", _cameraConstraint);
        ImGui.LabelText("Camera constraint valid", _cameraConstraintValid.ToString());

        ImGui.SeparatorText("Flags");
        ImGui.LabelText("Doing rotate camera", DoingRotateCamera.ToString());
        ImGui.LabelText("Doing zoom camera", DoingZoomCamera.ToString());
        ImGui.LabelText("Doing pitch camera", DoingPitchCamera.ToString());
        ImGui.LabelText("Doing scripted camera lock", _doingScriptedCameraLock.ToString());
        ImGui.LabelText("Camera arrived at waypoint on path", _cameraArrivedAtWaypointOnPathFlag.ToString());

        ImGui.SeparatorText("Path");
        if (_path != null)
        {
            ImGui.LabelText("Current segment", _path.CurSegment.ToString());
            ImGui.LabelText("Current segment distance", _path.CurSegDistance.ToString());
            ImGui.LabelText("Total distance", _path.TotalDistance.ToString());
            ImGui.LabelText("Elapsed time", _path.ElapsedTimeMilliseconds.ToString());
            ImGui.LabelText("Shutter", _path.CurShutter.ToString());
            ImGui.LabelText("Num waypoints", _path.NumWaypoints.ToString());
            ImGui.LabelText("Rolling average frames", _path.RollingAverageFrames.ToString());

            if (ImGui.Button("Reset path"))
            {
                _path.Reset();
            }
        }
    }

    // C++: /W3DDevice/GameClient/W3DView.cpp -> normAngle
    // Note that this is different from the normalizeAngle function from GameCommon.cpp
    private static float NormalizeAngle(float angle)
    {
        if (angle < -10.0f * MathF.PI)
        {
            angle = 0;
        }

        if (angle > 10.0f * MathF.PI)
        {
            angle = 0;
        }

        while (angle < -MathF.PI)
        {
            angle += MathUtility.TwoPi;
        }

        while (angle > MathF.PI)
        {
            angle -= MathUtility.TwoPi;
        }

        return angle;
    }
}

public enum LockType
{
    Follow,
    Tether
}
