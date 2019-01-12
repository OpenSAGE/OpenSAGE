using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Mathematics;
using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Cameras
{
    public sealed class RtsCameraController : ICameraController
    {
        private const float RotationSpeed = 0.003f;
        private const float ZoomSpeed = 0.0005f;
        private const float PanSpeed = 3f;

        private readonly float _defaultHeight;
        private readonly float _pitchAngle;

        private CameraAnimation _animation;

        public bool CanPlayerInputChangePitch { get; set; }
        
        private Vector3 _lookDirection;
        public void SetLookDirection(Vector3 lookDirection)
        {
            _lookDirection = Vector3.Normalize(new Vector3(lookDirection.X, lookDirection.Y, 0));
        }

        public float Pitch { get; set; } = 1;

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
                points,
                _lookDirection,
                startTime,
                duration,
                Pitch,
                _zoom);
        }

        public CameraAnimation CurrentAnimation => _animation;

        public RtsCameraController(ContentManager contentManager)
        {
            var iniDataContext = contentManager.IniDataContext;

            _defaultHeight = iniDataContext.GameData.DefaultCameraMaxHeight > 0
                ? iniDataContext.GameData.DefaultCameraMaxHeight
                : iniDataContext.GameData.CameraHeight;
            _pitchAngle = MathUtility.ToRadians(90 - iniDataContext.GameData.CameraPitch);

            var yaw = iniDataContext.GameData.CameraYaw;
            SetLookDirection(new Vector3(
                MathUtility.Sin(yaw),
                MathUtility.Cos(yaw),
                0));
        }

        public void EndAnimation()
        {
            if (_animation != null)
            {
                _animation.Finished = true;
                _animation = null;
            }
        }

        void ICameraController.ModSetFinalPitch(float finalPitch)
        {
            CurrentAnimation.SetFinalPitch(finalPitch);
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

        void ICameraController.UpdateCamera(Camera camera, in CameraInputState inputState, in GameTime gameTime)
        {
            if (inputState.LeftMouseDown && inputState.PressedKeys.Contains(Key.AltLeft) || inputState.MiddleMouseDown)
            {
                RotateCamera(inputState.DeltaX, inputState.DeltaY);
            }

            ZoomCamera(-inputState.ScrollWheelValue);

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

            if (_animation != null)
            {
                _animation.Update(this, gameTime);

                if (_animation.Finished)
                {
                    _animation = null;
                }
            }

            var yaw = MathUtility.Atan2(_lookDirection.Y, _lookDirection.X);

            var pitch = MathUtility.Lerp(
                0,
                -_pitchAngle,
                Pitch);

            var cameraHeight = MathUtility.Lerp(
                0,
                _defaultHeight,
                _zoom);

            float clampedPitch = pitch;
            if (pitch > 0 && pitch < _pitchAngle)
            {
                clampedPitch = _pitchAngle;
            }
            else if (pitch < 0 && pitch > -_pitchAngle)
            {
                clampedPitch = -_pitchAngle;
            }

            var cameraToTerrainDirection = Vector3.Normalize(new Vector3(
                MathUtility.Cos(yaw),
                MathUtility.Sin(yaw),
                MathUtility.Sin(clampedPitch)));

            // Back up camera from terrain position.
            var toCameraRay = new Ray(_terrainPosition, -cameraToTerrainDirection);
            var plane = Plane.CreateFromVertices(
                new Vector3(0, 0, cameraHeight),
                new Vector3(0, 1, cameraHeight),
                new Vector3(1, 0, cameraHeight));
            var toCameraIntersectionDistance = toCameraRay.Intersects(plane).Value;
            var newPosition = _terrainPosition - cameraToTerrainDirection * toCameraIntersectionDistance;

            // Pitch - 0 means top-down view.
            // Pitch between 0 and CameraPitch = Move camera position to match pitch.
            // Pitch between CameraPitch and horizontal = Raise or lower target height.

            var lookDirection = new Vector3(
                MathUtility.Cos(yaw),
                MathUtility.Sin(yaw),
                MathUtility.Sin(pitch));

            var targetPosition = newPosition + lookDirection;

            camera.SetLookAt(
                newPosition,
                targetPosition,
                Vector3.UnitZ);
        }

        private void RotateCamera(float deltaX, float deltaY)
        {
            var yaw = MathUtility.Atan2(_lookDirection.Y, _lookDirection.X);
            yaw -= deltaX * RotationSpeed;
            _lookDirection.X = MathUtility.Cos(yaw);
            _lookDirection.Y = MathUtility.Sin(yaw);

            if (CanPlayerInputChangePitch)
            {
                var maxPitch = 90.0f / _pitchAngle;

                var newPitch = Pitch + deltaY * RotationSpeed;
                if (newPitch < 0)
                    newPitch = 0;
                else if (newPitch > maxPitch)
                    newPitch = maxPitch;
                Pitch = newPitch;
            }
        }

        private void ZoomCamera(float deltaY)
        {
            Zoom = _zoom + deltaY * ZoomSpeed;
        }

        private void PanCamera(float forwards, float right)
        {
            var panSpeed = PanSpeed * _zoom;

            _terrainPosition += _lookDirection * forwards * panSpeed;

            // Get "right" vector from look direction.

            var yaw = MathUtility.Atan2(_lookDirection.Y, _lookDirection.X);

            var cameraOrientation = Matrix4x4.CreateFromQuaternion(QuaternionUtility.CreateLookRotation(_lookDirection));

            _terrainPosition += cameraOrientation.Right() * right * panSpeed;
        }
    }
}
