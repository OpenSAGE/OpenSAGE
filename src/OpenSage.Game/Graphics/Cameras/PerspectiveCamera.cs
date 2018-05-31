using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras
{
    public class PerspectiveCamera : Camera
    {
        private readonly Game _game;

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
                SetProjectionDirty();
            }
        }

        public PerspectiveCamera(Game game)
        {
            _game = game;
        }

        protected override void CreateProjection(out Matrix4x4 projection)
        {
            const int height = 24; // Height in mm of 35mm film.
            var fieldOfView = 2 * MathUtility.Atan(0.5f * height / _focalLength);

            projection = Matrix4x4.CreatePerspectiveFieldOfView(
                fieldOfView, _game.Viewport.Width / _game.Viewport.Height,
                NearPlaneDistance, FarPlaneDistance);
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

        internal Rectangle? WorldToScreenRectangle(in Vector3 position, in Size screenSize)
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

        internal void OnViewportSizeChanged()
        {
            SetProjectionDirty();
        }
    }
}
