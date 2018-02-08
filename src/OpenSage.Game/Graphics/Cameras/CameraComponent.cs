using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras
{
    public sealed class CameraComponent
    {
        private readonly Game _game;

        private readonly BoundingFrustum _frustum;

        private float _nearPlaneDistance;
        private float _farPlaneDistance;

        private Matrix4x4? _cachedProjectionMatrix;

        private bool _boundingFrustumDirty = true;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public CameraComponent(Game game)
        {
            _game = game;
            _frustum = new BoundingFrustum(Matrix4x4.Identity);

            NearPlaneDistance = 0.125f;
            FarPlaneDistance = 10000.0f;
        }

        /// <summary>
        /// Gets or sets a value that specifies the distance from the camera of the camera's far clip plane.
        /// </summary>
        public float FarPlaneDistance
        {
            get { return _farPlaneDistance; }
            set
            {
                _farPlaneDistance = value;
                ClearCachedProjectionMatrix();
            }
        }

        /// <summary>
        /// Gets or sets a value that specifies the distance from the camera of the camera's near clip plane.
        /// </summary>
        public float NearPlaneDistance
        {
            get { return _nearPlaneDistance; }
            set
            {
                _nearPlaneDistance = value;
                ClearCachedProjectionMatrix();
            }
        }

        /// <summary>
        /// Gets the frustum covered by this camera. The frustum is calculated from the camera's
        /// view and projection matrices.
        /// </summary>
        public BoundingFrustum BoundingFrustum
        {
            get
            {
                if (_boundingFrustumDirty)
                {
                    _frustum.Matrix = View * Projection;
                    _boundingFrustumDirty = false;
                }
                return _frustum;
            }
        }

        private Matrix4x4 _view;

        /// <summary>
        /// Gets the View matrix for this camera.
        /// </summary>
        public Matrix4x4 View
        {
            get => _view;
            set
            {
                _view = value;
                _boundingFrustumDirty = true;
            }
        }

        internal void OnViewportSizeChanged()
        {
            ClearCachedProjectionMatrix();
        }

        /// <summary>
        /// Gets the Projection matrix for this camera.
        /// </summary>
        public Matrix4x4 Projection
        {
            get
            {
                if (_cachedProjectionMatrix == null)
                {
                    const int height = 24; // Height in mm of 35mm film.
                    var fieldOfView = 2 * MathUtility.Atan(0.5f * height / _focalLength);

                    _cachedProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                        fieldOfView, _game.Viewport.Width / _game.Viewport.Height,
                        NearPlaneDistance, FarPlaneDistance);
                }

                return _cachedProjectionMatrix.Value;
            }
        }

        private float _focalLength = 25;

        /// <summary>
        /// Gets or sets a value that represents the camera's focal length in mm. 
        /// </summary>
        public float FocalLength
        {
            get { return _focalLength; }
            set
            {
                _focalLength = value;
                ClearCachedProjectionMatrix();
            }
        }
        
        private void ClearCachedProjectionMatrix()
        {
            _cachedProjectionMatrix = null;
            _boundingFrustumDirty = true;
        }

        /// <summary>
        /// Creates a ray going from the camera's near plane, through the specified screen position, to the camera's far plane.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Ray ScreenPointToRay(Vector2 position)
        {
            var near = _game.Viewport.Unproject(new Vector3(position, 0), Projection, View, Matrix4x4.Identity);
            var far = _game.Viewport.Unproject(new Vector3(position, 1), Projection, View, Matrix4x4.Identity);

            return new Ray(near, Vector3.Normalize(far - near));
        }

        /// <summary>
        /// Converts a world-space point to screen-space.
        /// </summary>
        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            return _game.Viewport.Project(position, Projection, View, Matrix4x4.Identity);
        }

        internal Rectangle? WorldToScreenRectangle(Vector3 position, Size screenSize)
        {
            var screenPosition = WorldToScreenPoint(position);

            // Check if point is behind camera, or too far away.
            if (screenPosition.Z < _game.Viewport.MinDepth || screenPosition.Z > _game.Viewport.MaxDepth)
                return null;

            return new Rectangle(
                (int) (screenPosition.X - screenSize.Width / 2.0f),
                (int) (screenPosition.Y - screenSize.Height / 2.0f),
                screenSize.Width, screenSize.Height);
        }

        /// <summary>
        /// Converts a screen-space point to world-space. The value of <paramref name="position.Z"/>
        /// will be used to determine the depth in camera space of the world-space point.
        /// </summary>
        public Vector3 ScreenToWorldPoint(Vector3 position)
        {
            var ray = ScreenPointToRay(new Vector2(position.X, position.Y));
            return ray.Position + ray.Direction * position.Z;
        }
    }
}
