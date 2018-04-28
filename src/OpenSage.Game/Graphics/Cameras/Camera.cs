using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras
{
    public abstract class Camera
    {
        private float _nearPlaneDistance;
        private float _farPlaneDistance;

        private Matrix4x4 _world, _projection;

        protected Camera()
        {
            BoundingFrustum = new BoundingFrustum(Matrix4x4.Identity);

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
                UpdateProjection();
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
                UpdateProjection();
            }
        }

        /// <summary>
        /// Gets the frustum covered by this camera. The frustum is calculated from the camera's
        /// view and projection matrices.
        /// </summary>
        public BoundingFrustum BoundingFrustum { get; }

        /// <summary>
        /// Gets the View matrix for this camera.
        /// </summary>
        public Matrix4x4 View { get; private set; }

        /// <summary>
        /// Gets the Projection matrix for this camera.
        /// </summary>
        public Matrix4x4 Projection => _projection;

        public Matrix4x4 ViewProjection { get; private set; }

        public Vector3 Position { get; private set; }

        public void SetLookAt(in Vector3 cameraPosition, in Vector3 cameraTarget, in Vector3 up)
        {
            View = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, up);
            _world = Matrix4x4Utility.Invert(View);
            Position = cameraPosition;

            UpdateViewProjection();
        }

        private void UpdateViewProjection()
        {
            ViewProjection = View * _projection;
            BoundingFrustum.Matrix = ViewProjection;
        }

        protected void UpdateProjection()
        {
            CreateProjection(out _projection);
            UpdateViewProjection();
        }

        protected abstract void CreateProjection(out Matrix4x4 projection);
    }
}
