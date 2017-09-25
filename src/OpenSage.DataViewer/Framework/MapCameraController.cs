using System.Numerics;
using OpenSage.Graphics;
using OpenSage.Mathematics;

namespace OpenSage.DataViewer.Framework
{
    public sealed class MapCameraController : CameraController
    {
        private const float DefaultDistance = 300;
        private static readonly float DefaultPitch = -MathUtility.Pi / 4;

        private const float RotationSpeed = 0.003f;
        private const float ZoomSpeed = 0.002f;
        private const float PanSpeed = 0.2f;

        private readonly Camera _camera;

        private Vector3 _target;

        private float _yaw;
        private float _zoom;
        private Vector3 _translation;

        public MapCameraController(Camera camera)
        {
            _camera = camera;
        }

        private void UpdateCamera()
        {
            var position = Vector3.Transform(
                -Vector3.UnitY,
                QuaternionUtility.CreateFromYawPitchRoll_ZUp(_yaw, DefaultPitch, 0));
            position *= _zoom * DefaultDistance;
            position += _target;

            _camera.Position = position + _translation;
            _camera.Target = _target + _translation;
        }

        public void Reset(Vector3 target)
        {
            _target = target;

            _yaw = 0;
            _zoom = 1;
            _translation = Vector3.Zero;

            UpdateCamera();
        }

        public override void OnLeftMouseButtonDragged(float deltaX, float deltaY)
        {
            _yaw += deltaX * RotationSpeed;

            UpdateCamera();
        }

        public override void OnMiddleMouseButtonDragged(float deltaY)
        {
            const float minZoom = 0.01f;

            var newZoom = _zoom - deltaY * ZoomSpeed;
            if (newZoom < minZoom)
                newZoom = minZoom;
            _zoom = newZoom;

            UpdateCamera();
        }

        public override void OnRightMouseButtonDragged(float deltaX, float deltaY)
        {
            var cameraOrientation = QuaternionUtility.CreateFromYawPitchRoll_ZUp(
                _yaw,
                DefaultPitch,
                0);

            var panSpeed = PanSpeed * _zoom;

            var newTranslation = Vector3.Zero;
            newTranslation += Vector3.Transform(Vector3.UnitX, cameraOrientation) * deltaX * panSpeed;
            newTranslation += Vector3.Transform(-Vector3.UnitZ, cameraOrientation) * deltaY * panSpeed;

            newTranslation.Z = 0;

            _translation += newTranslation;

            UpdateCamera();
        }
    }
}
