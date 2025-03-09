#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Scripting;
using OpenSage.Terrain;

namespace OpenSage.Graphics.Cameras;

public sealed class RtsCameraController : ICameraController, IPersistableObject
{
    // C++: SCROLL_AMT
    private const float ScrollAmount = 100.0f;
    private const float RotationSpeed = 0.003f;
    private const float ZoomSpeed = 0.0005f;

    private readonly GameData _gameData;
    private readonly Camera _camera;
    private readonly HeightMap _heightMap;

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
            ResetPathAndFlags();
            SetCameraTransform();
        }
    }

    /// <summary>
    /// Additional pitch angle, used for special effects.
    /// </summary>
    private float _fxPitch;

    // In C++ these are initialised in Init()
    private readonly float _minZoom = 0.2f;
    private readonly float _maxZoom = 1.3f;
    private float _zoom = 1;
    public float Zoom
    {
        get => _zoom;
        // Combination of View::setZoom and W3DView::setZoom
        set
        {
            _zoom = Math.Clamp(value, _minZoom, _maxZoom);
            ResetPathAndFlags();
            SetCameraTransform();
        }
    }

    private float _defaultAngle;
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
            _angle = MathUtility.NormalizeAngle(value);
            ResetPathAndFlags();
            SetCameraTransform();
        }
    }


    private float _groundLevel = 10.0f;
    private float _heightAboveGround;
    private float _minHeightAboveGround;
    private float _maxHeightAboveGround;

    [Obsolete]
    private CameraAnimation? _animation;

    public bool CanPlayerInputChangePitch { get; set; }

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


    private Vector3 _cameraOffset;

    public Vector3 Position
    {
        get { return _pos; }
        set { _pos = new Vector3(value.X, value.Y, 0); }
    }

    private int _timeMultiplier = 1;

    private Vector2 _scrollAmount;

    // Original has separate m_doingMoveCameraOnWaypointPath and m_mcwpInfo fields,
    // but let's use a nullable field instead.
    private MoveAlongWaypointPathInfo? _path;
    private bool _cameraArrivedAtWaypointOnPathFlag;

    private bool _doingRotateCamera;
    private bool _doingZoomCamera;
    private bool _doingPitchCamera;
    private bool _doingScriptedCameraLock;

    private RectangleF _cameraConstraint;
    private bool _cameraConstraintValid;

    // These should only be modified by BuildCameraTransform
    // They are here so that we can show them in the inspector
    private Vector3 _sourcePos;
    private Vector3 _targetPos;

    public GameObject? CameraLock { get; set; }

    public CameraAnimation StartAnimation(IReadOnlyList<Vector3> points, TimeSpan startTime, TimeSpan duration)
    {
        EndAnimation();

        return _animation = new CameraAnimation(
            this,
            points,
            GetLookDirection(),
            startTime,
            duration,
            _pitchAngle,
            _zoom,
            _camera.FieldOfView);
    }

    public CameraAnimation? CurrentAnimation => _animation;

    public RtsCameraController(GameData gameData, Camera camera, HeightMap heightMap)
    {
        _gameData = gameData;
        _camera = camera;
        _heightMap = heightMap;

        SetDefaultView(0, 0, 1);
        Init();

        // This should be called every time a new map is loaded
        // Since we re-create CameraController every time a new map is loaded, this should be basically the same
        InitHeightForMap();
        SetAngleAndPitchToDefault();
        SetZoomToDefault();
    }

    void Init()
    {
        // Since we only call Init once and fields are zero-initialized by default, I don't think this really does anything in C#
        _pos = Vector3.Zero;
        _angle = 0.0f;

        var defaultLookAtPoint = new Vector3(
            87.0f,
            77.0f,
            0.0f
        ) * HeightMap.HorizontalScale;
        _pos = defaultLookAtPoint;
        SetCameraTransform();
    }

    public void InitHeightForMap()
    {
        _groundLevel = _heightMap.GetHeight(_pos.X, _pos.Y);
        const float MaxGroundLevel = 120.0f;
        _groundLevel = Math.Min(_groundLevel, MaxGroundLevel);

        _cameraOffset.Z = _groundLevel + _gameData.CameraHeight;
        _cameraOffset.Y = -(_cameraOffset.Z / MathF.Tan(_gameData.CameraPitch * (MathF.PI / 180.0f)));
        _cameraOffset.X = -(_cameraOffset.Y * MathF.Tan(_gameData.CameraYaw * (MathF.PI / 180.0f)));
        _cameraConstraintValid = false;
        SetCameraTransform();
    }

    public void SetDefaultView(float pitch, float angle, float maxHeight)
    {
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
    void ResetPathAndFlags()
    {
        _path = null;
        _cameraArrivedAtWaypointOnPathFlag = false;
        _doingRotateCamera = false;
        _doingPitchCamera = false;
        _doingZoomCamera = false;
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

        ResetPathAndFlags();
        SetCameraTransform();
    }

    float GetHeightAroundPos(Vector2 pos)
    {
        const float terrainSampleSize = 40.0f;

        var terrainHeight = _heightMap.GetHeight(pos.X, pos.Y);
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
            var sample = _heightMap.GetHeight(samplePos.X, samplePos.Y);
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
        ResetPathAndFlags();
        SetCameraTransform();
    }

    public void SetPitch(float pitch)
    {
        Pitch = pitch;
    }

    public void EndAnimation()
    {
        if (_animation != null)
        {
            _animation.Finished = true;
            _animation = null;
        }
    }

    void ICameraController.ModSetFinalPitch(float finalPitch, float easeInPercentage, float easeOutPercentage)
    {
        CurrentAnimation?.SetFinalPitch(finalPitch, easeInPercentage, easeOutPercentage);
    }

    void ICameraController.ModSetFinalZoom(float finalZoom)
    {
        CurrentAnimation?.SetFinalZoom(finalZoom);
    }

    void ICameraController.ModFinalLookToward(in Vector3 position)
    {
        CurrentAnimation?.SetFinalLookToward(position);
    }

    void ICameraController.ModLookToward(in Vector3 position)
    {
        CurrentAnimation?.SetLookToward(position);
    }

    private static float GetKeyMovement(in CameraInputState inputState, Veldrid.Key positive, Veldrid.Key negative)
    {
        if (inputState.PressedKeys.Contains(positive))
            return 1;

        if (inputState.PressedKeys.Contains(negative))
            return -1;

        return 0;
    }

    void MoveCameraAlongWaypointPath(Waypoint? waypoint, int milliseconds, int shutter, bool orient, float easeIn, float easeOut)
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
        // The original code was designed for fixed 30 FPS time step, so originally shutter was divided by 33.3333
        // (so originally the shutter field was in frames, not milliseconds).
        // However we want to support arbitrary frame rates for the camera movement, so we'll leave it as is and handle it in the animation.
        path.Shutter = shutter;
        // And Generals ensures that shutter is at least one 30 FPS frame long, so we'll do the same.
        path.Shutter = Math.Max(path.Shutter, 33);
        path.Ease.SetEaseTimes(easeIn / milliseconds, easeOut / milliseconds);

        // Iterate through the waypoints and add them to the path.
        while (waypoint != null && path.NumWaypoints < MoveAlongWaypointPathInfo.MaxWaypoints)
        {
            path.NumWaypoints++;
            path.Waypoints[path.NumWaypoints] = waypoint.Position;

            if (waypoint.ConnectedWaypoints.Count > 0)
            {
                // Interesting: it was previously assumed that given a waypoint with many connected waypoints,
                // it would pick one at random. However, Generals always picks the first one.
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
    }

    // Port note: in C++ this function doesn't receie the path as a parameter and instead reads it from a field
    // However I think it makes sense to finish setting up the path before it's assigned to the field

    void SetupWaypointPath(MoveAlongWaypointPathInfo path, bool orient, Scene3D scene)
    {
        path.CurSegment = 1;
        path.CurSegDistance = 0;
        path.TotalDistance = 0;
        path.RollingAverageFrames = 1;

        var angle = _angle;

        for (var i = 0; i < path.NumWaypoints; i++)
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

                // TODO: Check if this applies to OpenSAGE
                // Default camera is rotated 90 degrees, so match
                angle -= MathUtility.PiOver2;
                angle = MathUtility.NormalizeAngle(angle);
            }

            path.CameraAngle[i] = angle;
        }

        path.CameraAngle[1] = _angle;
        path.CameraAngle[path.NumWaypoints] = path.CameraAngle[path.NumWaypoints - 1];

        for (var i = path.NumWaypoints; i > 1; i--)
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
        // TODO: Original assumes this never fails, but ours retruns a nullable float
        // Is this correct behavior, or should we throw as well?
        var newGround = scene.Terrain.GetGroundHeight(finalPos.Vector2XY()) ?? 0.0f;

        for (var i = 0; i < path.NumWaypoints + 1; i++)
        {
            var factor2 = curDistance / path.TotalDistance;
            var factor1 = 1.0f - factor2;
            path.TimeMultiplier[i] = _timeMultiplier;
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
            path.ElapsedTimeMilliseconds = 0;
            path.CurShutter = path.Shutter;
            _path = path;
        }
        _cameraArrivedAtWaypointOnPathFlag = false;
        _doingRotateCamera = false;
    }

    void MoveAlongWaypointPath(in TimeInterval gameTime)
    {
        // TODO(Port): Add equivalent to TheGlobalData->m_disableCameraMovement?

        if (_path == null)
        {
            // This should be unreachable, but just in case
            return;
        }

        _path.ElapsedTimeMilliseconds += gameTime.DeltaTime.Milliseconds;


        if (_path.ElapsedTimeMilliseconds > _path.TotalTimeMilliseconds)
        {
            // We've reached the end of the path
            _cameraArrivedAtWaypointOnPathFlag = true;
            // TODO(Port)
            //m_freezeTimeForCameraMovement = false;
            _angle = _path.CameraAngle[_path.NumWaypoints];
            _groundLevel = _path.GroundHeight[_path.NumWaypoints];
            _cameraOffset = new Vector3(
                -(_cameraOffset.Z / MathF.Tan(_gameData.CameraPitch * (MathF.PI / 180.0f))),
                -(_cameraOffset.Y / MathF.Tan(_gameData.CameraYaw * (MathF.PI / 180.0f))),
                _cameraOffset.Z
            );

            var pos = _path.Waypoints[_path.NumWaypoints];
            pos.Z = 0.0f;
        }
    }

    void ICameraController.UpdateCamera(Camera camera, in CameraInputState inputState, in TimeInterval gameTime)
    {
        // TODO: Old code, mostly not ported yet

        if (inputState.LeftMouseDown && inputState.PressedKeys.Contains(Veldrid.Key.AltLeft) || inputState.MiddleMouseDown)
        {
            RotateCamera(inputState.DeltaX, inputState.DeltaY);
        }
        else
        {
            var offset = Vector2.Zero;

            // tested in Zero Hour - rotation always takes precedence, and all panning is halted when rotating.
            if (inputState.RightMouseDown)
            {
                offset.Y = -inputState.DeltaY;
                offset.X = inputState.DeltaX;
            }
            else
            {
                offset.Y = GetKeyMovement(inputState, Veldrid.Key.Down, Veldrid.Key.Up) * _gameData.VerticalScrollSpeedFactor;
                offset.X = GetKeyMovement(inputState, Veldrid.Key.Right, Veldrid.Key.Left) * _gameData.HorizontalScrollSpeedFactor;
            }

            // TODO: Integrate this with settings
            // There is an INI field for this (_gameData.KeyboardScrollSpeedFactor)
            // However, Generals seems to always overrides it with a value from player options INI file
            // We'll use the default speed for now
            var keyboardScrollSpeedFactor = 0.6f;

            ScrollBy(
                offset * ScrollAmount * keyboardScrollSpeedFactor,
                gameTime
            );
        }

        ZoomCamera(-inputState.ScrollWheelValue);
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
        var mapRegion = _heightMap.Extents;
        var maxEdgeZ = _groundLevel;

        var screenCenter = new Vector2(
            MathF.Floor(_camera.ScreenSize.X / 2),
            MathF.Floor(_camera.ScreenSize.Y / 2)
        );

        var (centerRayStart, centerRayEnd) = _camera.GetPickRay(screenCenter);

        var center = new Vector3(
            Vector3Utility.FindXAtZ(maxEdgeZ, centerRayStart, centerRayEnd),
            Vector3Utility.FindYAtZ(maxEdgeZ, centerRayStart, centerRayEnd),
            maxEdgeZ
        );

        var screenBottom = new Vector2(
            screenCenter.X,
            MathF.Floor(_camera.ScreenSize.Y * 0.95f)
        );
        var (bottomRayStart, bottomRayEnd) = _camera.GetPickRay(screenBottom);

        var bottom = new Vector3(
            Vector3Utility.FindXAtZ(maxEdgeZ, bottomRayStart, bottomRayEnd),
            Vector3Utility.FindYAtZ(maxEdgeZ, bottomRayStart, bottomRayEnd),
            maxEdgeZ
        );

        center.X -= bottom.X;
        center.Y -= bottom.Y;

        var offset = center.Length();
        var topleft = new Vector2(
            mapRegion.Left + offset,
            mapRegion.Top + offset
        );
        var bottomRight = new Vector2(
            mapRegion.Right - offset,
            mapRegion.Bottom - offset
        );
        _cameraConstraint = new RectangleF(
            topleft.X,
            topleft.Y,
            bottomRight.X - topleft.X,
            bottomRight.Y - topleft.Y
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
        _camera.SetLookAt(sourcePos, targetPos, Vector3.UnitZ);

        // TODO(Port):
        // CameraShakerSystem.Timestep(1.0f / 30.0f);
        // CameraShakerSystem.Update_Camera_Shaker(sourcePos, &m_shakerAngles);
        // transform->Rotate_X(m_shakerAngles.X);
        // transform->Rotate_Y(m_shakerAngles.Y);
        // transform->Rotate_Z(m_shakerAngles.Z);

        // C++ checks for m_isCameraSlaved
        // From what I can tell that is related to 3DS Max integration, which we don't need
    }

    private void RotateCamera(float deltaX, float deltaY)
    {
        _angle -= deltaX * RotationSpeed;

        if (CanPlayerInputChangePitch)
        {
            var minPitch = -MathUtility.PiOver2;
            var maxPitch = MathUtility.PiOver2;

            var newPitch = Math.Clamp(_pitchAngle + deltaY * RotationSpeed, minPitch, maxPitch);

            _pitchAngle = newPitch;
        }
    }

    private void ZoomCamera(float deltaY)
    {
        // TODO(Port): Generals adjusts zooms over time

        if (deltaY == 0)
        {
            return;
        }

        Zoom = _zoom + deltaY * ZoomSpeed;
    }

    private void ScrollBy(Vector2 delta, in TimeInterval gameTime)
    {
        if (delta.LengthSquared() == 0)
        {
            return;
        }

        const float scrollResolution = 250.0f;
        _scrollAmount = delta;

        var screenSize = _camera.ScreenSize;
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
        var worldStart = _camera.ScreenToWorldPoint(new Vector3(start, 0.0f));
        var worldEnd = _camera.ScreenToWorldPoint(new Vector3(end, 0.0f));
        var world = worldEnd - worldStart;
        // ...which we do here
        world *= 0.1f;

        // Original camera movement was designed for fixed 30 FPS time step, so we'll have to scale it with delta time
        var deltaTimeScale = new Vector3(gameTime.DeltaTime.Milliseconds / 33.3333f);
        world *= deltaTimeScale;

        _pos.X += world.X;
        _pos.Y += world.Y;

        _doingRotateCamera = false;
        SetCameraTransform();
    }

    public void GoToObject(Logic.Object.GameObject gameObject)
    {
        Position = gameObject.Translation;
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
        LabelTextWithHex("Ground level", _groundLevel);

        ImGui.SeparatorText("Computed values");
        LabelTextWithHexVector3("Source position", _sourcePos);
        LabelTextWithHexVector3("Target position", _targetPos);
        LabelTextWithHexRectangleF("Camera constraint", _cameraConstraint);
        ImGui.LabelText("Camera constraint valid", _cameraConstraintValid.ToString());

        ImGui.SeparatorText("Flags");
        ImGui.LabelText("Doing rotate camera", _doingRotateCamera.ToString());
        ImGui.LabelText("Doing zoom camera", _doingZoomCamera.ToString());
        ImGui.LabelText("Doing pitch camera", _doingPitchCamera.ToString());
        ImGui.LabelText("Doing scripted camera lock", _doingScriptedCameraLock.ToString());
        ImGui.LabelText("Camera arrived at waypoint on path", _cameraArrivedAtWaypointOnPathFlag.ToString());
    }
}
