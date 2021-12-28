using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Sav;
using OpenSage.Mathematics;
using OpenSage.Terrain;
using Veldrid;

namespace OpenSage.Graphics.Cameras
{
    public sealed class RtsCameraController : ICameraController
    {
        private const float RotationSpeed = 0.003f;
        private const float ZoomSpeed = 0.0005f;
        private const float PanSpeed = 3f;

        private readonly Camera _camera;
        private readonly HeightMap _heightMap;

        private readonly float _defaultHeight;
        private readonly float _defaultPitchAngle;

        private float _currentPitchAngle;

        private CameraAnimation _animation;

        public bool CanPlayerInputChangePitch { get; set; }
        
        private float _yaw;
        public void SetLookDirection(Vector3 lookDirection)
        {
            _yaw = MathF.Atan2(lookDirection.Y, lookDirection.X);
        }
        public Vector3 GetLookDirection()
        {
            return new Vector3(
                MathF.Cos(_yaw),
                MathF.Sin(_yaw),
                0);
        }

        private float _zoom = 1;
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                const float minZoom = 0.01f;

                _zoom = value;
                if (_zoom < minZoom)
                    _zoom = minZoom;
            }
        }

        private Vector3 _terrainPosition;
        public Vector3 TerrainPosition
        {
            get { return _terrainPosition; }
            set { _terrainPosition = new Vector3(value.X, value.Y, 0); }
        }

        public CameraAnimation StartAnimation(IReadOnlyList<Vector3> points, TimeSpan startTime, TimeSpan duration)
        {
            EndAnimation();

            return _animation = new CameraAnimation(
                this,
                points,
                GetLookDirection(),
                startTime,
                duration,
                _currentPitchAngle,
                _zoom,
                _camera.FieldOfView);
        }

        public CameraAnimation CurrentAnimation => _animation;

        public RtsCameraController(GameData gameData, Camera camera, HeightMap heightMap)
        {
            _camera = camera;
            _heightMap = heightMap;

            _defaultHeight = gameData.DefaultCameraMaxHeight > 0
                ? gameData.DefaultCameraMaxHeight
                : gameData.CameraHeight;

            var defaultCameraPitchAngle = gameData.CameraPitch > 0
                ? gameData.CameraPitch
                : gameData.DefaultCameraPitchAngle;
            _defaultPitchAngle = MathUtility.ToRadians(90 - defaultCameraPitchAngle);

            _currentPitchAngle = -_defaultPitchAngle;

            _yaw = gameData.CameraYaw;
        }

        internal float CalculatePitchAngle(float pitch)
        {
            return MathUtility.Lerp(
                0,
                -_defaultPitchAngle,
                pitch);
        }

        public void SetPitch(float pitch)
        {
            _currentPitchAngle = CalculatePitchAngle(pitch);
        }

        public void SetPitchAngle(float pitchAngle)
        {
            _currentPitchAngle = pitchAngle;
        }

        internal void SetFieldOfView(float fieldOfView)
        {
            _camera.FieldOfView = fieldOfView;
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
            CurrentAnimation.SetFinalPitch(finalPitch, easeInPercentage, easeOutPercentage);
        }

        void ICameraController.ModSetFinalZoom(float finalZoom)
        {
            CurrentAnimation.SetFinalZoom(finalZoom);
        }

        void ICameraController.ModFinalLookToward(in Vector3 position)
        {
            CurrentAnimation.SetFinalLookToward(position);
        }

        void ICameraController.ModLookToward(in Vector3 position)
        {
            CurrentAnimation.SetLookToward(position);
        }

        private static float GetKeyMovement(in CameraInputState inputState, Key positive, Key negative)
        {
            if (inputState.PressedKeys.Contains(positive))
                return 1;

            if (inputState.PressedKeys.Contains(negative))
                return -1;

            return 0;
        }

        void ICameraController.UpdateCamera(Camera camera, in CameraInputState inputState, in TimeInterval gameTime)
        {
            if (inputState.LeftMouseDown && inputState.PressedKeys.Contains(Key.AltLeft) || inputState.MiddleMouseDown)
            {
                RotateCamera(inputState.DeltaX, inputState.DeltaY);
            }
            else
            {
                // tested in Zero Hour - rotation always takes precedence, and all panning is halted when rotating.
                float forwards, right;
                if (inputState.RightMouseDown)
                {
                    forwards = -inputState.DeltaY;
                    right = inputState.DeltaX;
                }
                else
                {
                    forwards = GetKeyMovement(inputState, Key.Up, Key.Down);
                    right = GetKeyMovement(inputState, Key.Right, Key.Left);
                }
                PanCamera(forwards, right);
            }

            ZoomCamera(-inputState.ScrollWheelValue);

            if (_animation != null)
            {
                _animation.Update(this, gameTime);

                if (_animation.Finished)
                {
                    _animation = null;
                }
            }

            var yaw = _yaw;

            var cameraHeight = MathUtility.Lerp(
                0,
                _defaultHeight,
                _zoom);

            // TODO: I think we're supposed to use this somehow, but I don't yet know exactly how.
            var cameraHeightAboveTerrain = cameraHeight - _heightMap.GetHeight(_terrainPosition.X, _terrainPosition.Y);

            var pitch = _currentPitchAngle;

            // Calculate "clamped" pitch, which is no higher than "normal" pitch.
            var clampedPitch = pitch;
            if (pitch > -_defaultPitchAngle)
            {
                clampedPitch = -_defaultPitchAngle;
            }

            // Compute terrain-position-to-camera vector, based on "clamped" pitch.
            var cameraToTerrainDirection = Vector3.Normalize(new Vector3(
                MathF.Cos(yaw),
                MathF.Sin(yaw),
                MathF.Sin(clampedPitch)));

            // Back up camera from terrain position.
            var toCameraRay = new Ray(_terrainPosition, -cameraToTerrainDirection);
            var plane = Plane.CreateFromVertices(
                new Vector3(0, 0, cameraHeight),
                new Vector3(0, 1, cameraHeight),
                new Vector3(1, 0, cameraHeight));
            var toCameraIntersectionDistance = toCameraRay.Intersects(plane).Value;
            var newPosition = _terrainPosition - cameraToTerrainDirection * toCameraIntersectionDistance;

            // Pitch: 1 is default.
            // [-x..0] - to the sky
            // [0..1]  - between default angle and horizontal
            // [1..x]  - more top-down than default angle

            // Pitch - 0 means top-down view.
            // Pitch between 0 and CameraPitch = Move camera position to match pitch.
            // Pitch between CameraPitch and horizontal = Raise or lower target height.

            var lookDirection = new Vector3(
                MathF.Cos(yaw),
                MathF.Sin(yaw),
                MathF.Sin(pitch));

            // World Builder shows this value for the "Look At point" height. But I don't know yet how it's applied.
            //var cameraHeightAboveTerrain = cameraHeight - _heightMap.GetHeight(_terrainPosition.X, _terrainPosition.Y);
            //var zOffset = pitch * -cameraHeightAboveTerrain;
            //newPosition.Z += zOffset;

            var targetPosition = newPosition + lookDirection;

            camera.SetLookAt(
                newPosition,
                targetPosition,
                Vector3.UnitZ);
        }

        private void RotateCamera(float deltaX, float deltaY)
        {
            _yaw -= deltaX * RotationSpeed;

            if (CanPlayerInputChangePitch)
            {
                var minPitch = -MathUtility.PiOver2;
                var maxPitch = MathUtility.PiOver2;

                var newPitch = Math.Clamp(_currentPitchAngle + deltaY * RotationSpeed, minPitch, maxPitch);

                _currentPitchAngle = newPitch;
            }
        }

        private void ZoomCamera(float deltaY)
        {
            Zoom = _zoom + deltaY * ZoomSpeed;
        }

        private void PanCamera(float forwards, float right)
        {
            var panSpeed = PanSpeed * _zoom;

            var lookDirection = GetLookDirection();

            _terrainPosition += lookDirection * forwards * panSpeed;

            // Get "right" vector from look direction.
            var cameraOrientation = Matrix4x4.CreateFromQuaternion(QuaternionUtility.CreateLookRotation(lookDirection));

            _terrainPosition += cameraOrientation.Right() * right * panSpeed;

            if (_terrainPosition.X < 0)
            {
                _terrainPosition.X = 0;
            }
            else if (_terrainPosition.X > _heightMap.MaxXCoordinate)
            {
                _terrainPosition.X = _heightMap.MaxXCoordinate;
            }

            if (_terrainPosition.Y < 0)
            {
                _terrainPosition.Y = 0;
            }
            else if (_terrainPosition.Y > _heightMap.MaxYCoordinate)
            {
                _terrainPosition.Y = _heightMap.MaxYCoordinate;
            }
        }

        public void GoToObject(Logic.Object.GameObject gameObject)
        {
            TerrainPosition = gameObject.Translation;
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            _yaw = reader.ReadSingle();
            reader.ReadVector3(ref _terrainPosition);
        }

        internal void DrawInspector()
        {
            var pitchDegrees = MathUtility.ToDegrees(_currentPitchAngle);
            if (ImGui.DragFloat("Pitch", ref pitchDegrees, 1, -90, 90))
            {
                _currentPitchAngle = MathUtility.ToRadians(pitchDegrees);
            }

            ImGui.DragFloat("Zoom", ref _zoom, 0.01f, 0.01f, 5);

            var fieldOfView = MathUtility.ToDegrees(_camera.FieldOfView);
            if (ImGui.DragFloat("Field of view", ref fieldOfView, 1f, 10, 400))
            {
                _camera.FieldOfView = MathUtility.ToRadians(fieldOfView);
            }

            var farPlaneDistance = _camera.FarPlaneDistance;
            if (ImGui.SliderFloat("Far plane distance", ref farPlaneDistance, 10, 10000))
            {
                _camera.FarPlaneDistance = farPlaneDistance;
            }

            ImGui.DragFloat3("Terrain position", ref _terrainPosition);

            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
            var cameraPosition = _camera.Position;
            ImGui.DragFloat3("Camera position", ref cameraPosition);
            ImGui.PopStyleVar();

            if (_animation != null)
            {
                if (ImGui.Button("End animation"))
                {
                    EndAnimation();
                }
            }
        }
    }
}
