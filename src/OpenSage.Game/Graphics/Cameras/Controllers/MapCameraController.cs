using System.Numerics;
using OpenSage.Input;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras.Controllers
{
    public sealed class MapCameraController : UpdateableComponent
    {
        private const float DefaultDistance = 300;
        private static readonly float DefaultPitch = -MathUtility.Pi / 4;

        private const float RotationSpeed = 0.003f;
        private const float ZoomSpeed = 0.002f;
        private const float PanSpeed = 0.2f;

        private Vector3 _target;

        private float _yaw;
        private float _zoom;
        private Vector3 _translation;

        protected internal override void Update(GameTime gameTime)
        {
            var deltaX = Input.GetAxis(MouseMovementAxis.XAxis);
            var deltaY = Input.GetAxis(MouseMovementAxis.YAxis);

            bool isMovementTypeActive(MouseButton button)
            {
                return Input.GetMouseButtonDown(button)
                    && !Input.GetMouseButtonPressed(button);
            }

            if (isMovementTypeActive(MouseButton.Left))
            {
                RotateCamera(deltaX, deltaY);
            }

            if (isMovementTypeActive(MouseButton.Middle))
            {
                ZoomCamera(deltaY);
            }

            if (isMovementTypeActive(MouseButton.Right))
            {
                PanCamera(deltaX, deltaY);
            }

            UpdateCamera();
        }

        private void UpdateCamera()
        {
            var position = Vector3.Transform(
                -Vector3.UnitY,
                QuaternionUtility.CreateFromYawPitchRoll_ZUp(_yaw, DefaultPitch, 0));
            position *= _zoom * DefaultDistance;
            position += _target;

            Entity.Transform.LocalPosition = position + _translation;
            Entity.Transform.LookAt(_target + _translation);
        }

        public void Reset(Vector3 target)
        {
            _target = target;

            _yaw = 0;
            _zoom = 1;
            _translation = Vector3.Zero;
        }

        private void RotateCamera(float deltaX, float deltaY)
        {
            _yaw -= deltaX * RotationSpeed;
        }

        private void ZoomCamera(float deltaY)
        {
            const float minZoom = 0.01f;

            var newZoom = _zoom + deltaY * ZoomSpeed;
            if (newZoom < minZoom)
                newZoom = minZoom;
            _zoom = newZoom;
        }

        private void PanCamera(float deltaX, float deltaY)
        {
            var cameraOrientation = QuaternionUtility.CreateFromYawPitchRoll_ZUp(
                _yaw,
                DefaultPitch,
                0);

            var panSpeed = PanSpeed * _zoom;

            var newTranslation = Vector3.Zero;
            newTranslation += Vector3.Transform(-Vector3.UnitX, cameraOrientation) * deltaX * panSpeed;
            newTranslation += Vector3.Transform(Vector3.UnitZ, cameraOrientation) * deltaY * panSpeed;

            newTranslation.Z = 0;

            _translation += newTranslation;
        }
    }
}
