using System;
using System.Numerics;

namespace OpenSage.DataViewer.Framework
{
    public sealed class ArcballCamera
    {
        private const float RotationSpeed = 0.003f;
        private const float ZoomSpeed = 0.001f;
        private const float PanSpeed = 0.05f;

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
                var result = Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, 0));
                result *= _zoom * _radius;
                result += _target;
                return result;
            }
        }

        public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(
            Position + _translation,
            _target + _translation,
            Vector3.UnitY);

        public void Reset(Vector3 target, float radius)
        {
            _target = target;
            _radius = radius;

            _yaw = 0;
            _pitch = -(float)Math.PI / 6.0f;
            _zoom = 1;
            _translation = Vector3.Zero;
        }

        public void Rotate(float deltaX, float deltaY)
        {
            _yaw += deltaX * RotationSpeed;

            const float minPitch = (float)(-Math.PI / 2.0f + 0.3f);
            const float maxPitch = (float)(Math.PI / 2.0f - 0.3f);

            var newPitch = _pitch + deltaY * RotationSpeed;
            if (newPitch < minPitch)
                newPitch = minPitch;
            else if (newPitch > maxPitch)
                newPitch = maxPitch;
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
            var cameraOrientation = Quaternion.CreateFromYawPitchRoll(
                _yaw, 
                _pitch, 
                0);

            _translation += Vector3.Transform(Vector3.UnitX, cameraOrientation) * deltaX * PanSpeed;
            _translation -= Vector3.Transform(Vector3.UnitY, cameraOrientation) * deltaY * PanSpeed;
        }
    }
}
