using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras
{
    public sealed class ArcballCameraController : ICameraController
    {
        private const float RotationSpeed = 0.003f;
        private const float ZoomSpeed = 0.001f;
        private const float PanSpeed = 0.05f;

        private static readonly float MinPitch = -MathUtility.PiOver2 + 0.3f;
        private static readonly float MaxPitch = MathUtility.PiOver2 - 0.3f;

        private readonly Vector3 _target;
        private readonly float _radius;

        private float _yaw;
        private float _pitch;
        private float _zoom;
        private Vector3 _translation;

        void ICameraController.SetPitch(float pitch) => throw new NotImplementedException();
        float ICameraController.Zoom { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Vector3 ICameraController.TerrainPosition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        CameraPanDirection ICameraController.PanDirection { get => throw new NotImplementedException(); }

        public ArcballCameraController(Vector3 target, float radius)
        {
            _target = target;
            _radius = radius;

            _yaw = 0;
            _pitch = -MathF.PI / 6.0f;
            _zoom = 1;
            _translation = Vector3.Zero;
        }

        private void RotateCamera(float deltaX, float deltaY)
        {
            _yaw -= deltaX * RotationSpeed;

            var newPitch = _pitch - deltaY * RotationSpeed;
            if (newPitch < MinPitch)
                newPitch = MinPitch;
            else if (newPitch > MaxPitch)
                newPitch = MaxPitch;
            _pitch = newPitch;
        }

        private void ZoomCamera(float deltaY)
        {
            const float minZoom = 0.1f;
            const float maxZoom = 1;

            var newZoom = _zoom + deltaY * ZoomSpeed;
            if (newZoom < minZoom)
                newZoom = minZoom;
            else if (newZoom > maxZoom)
                newZoom = maxZoom;
            _zoom = newZoom;
        }

        private void PanCamera(float deltaX, float deltaY)
        {
            var cameraOrientation = QuaternionUtility.CreateFromYawPitchRoll_ZUp(
                _yaw,
                _pitch,
                0);

            _translation += Vector3.Transform(-Vector3.UnitX, cameraOrientation) * deltaX * PanSpeed;
            _translation += Vector3.Transform(Vector3.UnitZ, cameraOrientation) * deltaY * PanSpeed;
        }

        void ICameraController.SetLookDirection(Vector3 lookDirection)
        {
            throw new NotImplementedException();
        }

        void ICameraController.ModSetFinalPitch(float finalPitch, float easeInPercentage, float easeOutPercentage)
        {
            throw new NotImplementedException();
        }

        void ICameraController.ModSetFinalZoom(float finalZoom)
        {
            throw new NotImplementedException();
        }

        void ICameraController.ModFinalLookToward(in Vector3 position)
        {
            throw new NotImplementedException();
        }

        public void ModLookToward(in Vector3 position)
        {
            throw new NotImplementedException();
        }

        CameraAnimation ICameraController.StartAnimation(IReadOnlyList<Vector3> points, TimeSpan startTime, TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        void ICameraController.EndAnimation()
        {
            throw new NotImplementedException();
        }

        void ICameraController.UpdateInput(in CameraInputState inputState, in TimeInterval gameTime)
        {
            if (inputState.LeftMouseDown)
            {
                RotateCamera(inputState.DeltaX, inputState.DeltaY);
            }

            if (inputState.RightMouseDown)
            {
                PanCamera(inputState.DeltaX, inputState.DeltaY);
            }

            ZoomCamera(-inputState.ScrollWheelValue);
        }

        void ICameraController.UpdateCamera(Camera camera, in TimeInterval gameTime)
        {
            var position = Vector3.Transform(
                -Vector3.UnitY,
                QuaternionUtility.CreateFromYawPitchRoll_ZUp(_yaw, _pitch, 0));
            position *= _zoom * _radius;
            position += _target;

            camera.SetLookAt(
                position + _translation,
                _target + _translation,
                Vector3.UnitZ);
        }

        public void GoToObject(GameObject gameObject) { }
    }
}
