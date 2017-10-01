using System.Numerics;
using OpenSage.Input;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras.Controllers
{
    public sealed class ArcballCameraController : UpdateableComponent
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

        public ArcballCameraController()
        {

        }

        public ArcballCameraController(Vector3 target, float radius)
        {
            Reset(target, radius);
        }

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
    }
}
