using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras.Controllers
{
    public abstract class CameraController : EntityComponent
    {
        public abstract void OnLeftMouseButtonDragged(float deltaX, float deltaY);
        public abstract void OnMiddleMouseButtonDragged(float deltaY);
        public abstract void OnRightMouseButtonDragged(float deltaX, float deltaY);
    }

    public sealed class ArcballCameraController : CameraController
    {
        private const float RotationSpeed = 0.003f;
        private const float ZoomSpeed = 0.001f;
        private const float PanSpeed = 0.05f;

        private readonly float MinPitch = -MathUtility.Pi / 2.0f + 0.3f;
        private readonly float MaxPitch = MathUtility.Pi / 2.0f - 0.3f;

        private Vector3 _target;
        private float _radius;

        private float _yaw;
        private float _pitch;
        private float _zoom;
        private Vector3 _translation;

        private void UpdateCamera()
        {
            var position = Vector3.Transform(
                -Vector3.UnitY,
                QuaternionUtility.CreateFromYawPitchRoll_ZUp(_yaw, _pitch, 0));
            position *= _zoom * _radius;
            position += _target;

            Entity.Transform.LocalPosition = position + _translation;
            Entity.Transform.LookAt(_target + _translation);
        }

        public void Reset(Vector3 target, float radius)
        {
            _target = target;
            _radius = radius;

            _yaw = 0;
            _pitch = -MathUtility.Pi / 6.0f;
            _zoom = 1;
            _translation = Vector3.Zero;

            UpdateCamera();
        }

        public override void OnLeftMouseButtonDragged(float deltaX, float deltaY)
        {
            _yaw += deltaX * RotationSpeed;

            var newPitch = _pitch + deltaY * RotationSpeed;
            if (newPitch < MinPitch)
                newPitch = MinPitch;
            else if (newPitch > MaxPitch)
                newPitch = MaxPitch;
            _pitch = newPitch;

            UpdateCamera();
        }

        public override void OnMiddleMouseButtonDragged(float deltaY)
        {
            const float minZoom = 0.1f;
            const float maxZoom = 1;

            var newZoom = _zoom - deltaY * ZoomSpeed;
            if (newZoom < minZoom)
                newZoom = minZoom;
            else if (newZoom > maxZoom)
                newZoom = maxZoom;
            _zoom = newZoom;

            UpdateCamera();
        }

        public override void OnRightMouseButtonDragged(float deltaX, float deltaY)
        {
            var cameraOrientation = QuaternionUtility.CreateFromYawPitchRoll_ZUp(
                _yaw,
                _pitch,
                0);

            _translation += Vector3.Transform(Vector3.UnitX, cameraOrientation) * deltaX * PanSpeed;
            _translation += Vector3.Transform(-Vector3.UnitZ, cameraOrientation) * deltaY * PanSpeed;

            UpdateCamera();
        }
    }
}
