using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.DataViewer.Framework
{
    public sealed class ArcballCamera
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

        public Vector3 Position
        {
            get
            {
                var result = Vector3.Transform(
                    -Vector3.UnitY, 
                    QuaternionUtility.CreateFromYawPitchRoll_ZUp(_yaw, _pitch, 0));
                result *= _zoom * _radius;
                result += _target;
                return result;
            }
        }

        public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(
            Position + _translation,
            _target + _translation,
            Vector3.UnitZ);

        public void Reset(Vector3 target, float radius)
        {
            _target = target;
            _radius = radius;

            _yaw = 0;
            _pitch = -MathUtility.Pi / 6.0f;
            _zoom = 1;
            _translation = Vector3.Zero;
        }

        public void Rotate(float deltaX, float deltaY)
        {
            _yaw += deltaX * RotationSpeed;

            var newPitch = _pitch + deltaY * RotationSpeed;
            if (newPitch < MinPitch)
                newPitch = MinPitch;
            else if (newPitch > MaxPitch)
                newPitch = MaxPitch;
            _pitch = newPitch;
        }

        public void Zoom(float deltaY)
        {
            const float minZoom = 0.1f;
            const float maxZoom = 1;

            var newZoom = _zoom - deltaY * ZoomSpeed;
            if (newZoom < minZoom)
                newZoom = minZoom;
            else if (newZoom > maxZoom)
                newZoom = maxZoom;
            _zoom = newZoom;
        }

        public void Pan(float deltaX, float deltaY)
        {
            var cameraOrientation = QuaternionUtility.CreateFromYawPitchRoll_ZUp(
                _yaw, 
                _pitch, 
                0);

            _translation += Vector3.Transform(Vector3.UnitX, cameraOrientation) * deltaX * PanSpeed;
            _translation += Vector3.Transform(-Vector3.UnitZ, cameraOrientation) * deltaY * PanSpeed;
        }
    }
}
