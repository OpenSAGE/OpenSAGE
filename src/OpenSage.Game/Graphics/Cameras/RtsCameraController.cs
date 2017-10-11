using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras
{
    public sealed class RtsCameraController
    {
        private readonly CameraComponent _camera;

        private float _defaultHeight;
        private float _pitchAngle;

        private bool _needsCameraUpdate = true;

        private Vector3 _lookDirection;
        public Vector3 LookDirection
        {
            get { return _lookDirection; }
            set
            {
                _lookDirection = value;
                _needsCameraUpdate = true;
            }
        }

        private float _pitch = 1;
        public float Pitch
        {
            get { return _pitch; }
            set
            {
                _pitch = value;
                _needsCameraUpdate = true;
            }
        }

        private float _zoom = 1;
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                _needsCameraUpdate = true;
            }
        }

        private Vector3 _cameraPosition;
        public Vector3 CameraPosition
        {
            get { return _cameraPosition; }
            set
            {
                _cameraPosition = value;
                _terrainPosition = null;
                _needsCameraUpdate = true;
            }
        }

        private Vector3? _terrainPosition;
        public Vector3? TerrainPosition
        {
            get { return _terrainPosition; }
            set
            {
                _terrainPosition = value;
                _needsCameraUpdate = true;
            }
        }

        public RtsCameraController(CameraComponent camera)
        {
            _camera = camera;
        }

        public void Initialize(Game game)
        {
            _defaultHeight = game.ContentManager.IniDataContext.GameData.CameraHeight;
            _pitchAngle = MathUtility.ToRadians(game.ContentManager.IniDataContext.GameData.CameraPitch);
        }

        public void UpdateCamera()
        {
            if (!_needsCameraUpdate)
            {
                return;
            }

            _needsCameraUpdate = false;

            var yaw = MathUtility.Atan2(_lookDirection.Y, _lookDirection.X);

            var pitch = MathUtility.Lerp(
                0,
                -_pitchAngle,
                _pitch);

            var lookDirection = new Vector3(
                MathUtility.Cos(yaw),
                MathUtility.Sin(yaw),
                MathUtility.Sin(pitch));

            lookDirection = Vector3.Normalize(lookDirection);

            Vector3 newPosition, targetPosition;

            var cameraHeight = MathUtility.Lerp(
                0,
                _defaultHeight,
                _zoom);

            if (_terrainPosition != null)
            {
                // Back up camera from camera "position".
                var toCameraRay = new Ray(_terrainPosition.Value, -lookDirection);
                var plane = Plane.CreateFromVertices(
                    new Vector3(0, 0, cameraHeight),
                    new Vector3(0, 1, cameraHeight),
                    new Vector3(1, 0, cameraHeight));
                var toCameraIntersectionDistance = toCameraRay.Intersects(ref plane).Value;
                newPosition = _terrainPosition.Value - lookDirection * toCameraIntersectionDistance;
                targetPosition = _terrainPosition.Value;
            }
            else
            {
                newPosition = _cameraPosition;
                newPosition.Z = cameraHeight;

                targetPosition = newPosition + lookDirection;
            }

            _camera.View = Matrix4x4.CreateLookAt(
                newPosition,
                targetPosition,
                Vector3.UnitZ);
        }
    }
}
