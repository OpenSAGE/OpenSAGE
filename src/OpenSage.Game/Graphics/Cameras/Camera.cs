using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras
{
    public abstract class Camera
    {
        private float _nearPlaneDistance;
        private float _farPlaneDistance;

        private Matrix4x4 _world, _projection, _viewProjection;
        private bool _isProjectionDirty;
        private bool _isViewProjectionDirty;

        private readonly BoundingFrustum _boundingFrustum;

        protected Camera()
        {
            _boundingFrustum = new BoundingFrustum(Matrix4x4.Identity);

            NearPlaneDistance = 0.125f;
            FarPlaneDistance = 10000.0f;

            SetProjectionDirty();
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
                SetProjectionDirty();
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
                SetProjectionDirty();
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
                EnsureViewProjection();
                return _boundingFrustum;
            }
        }

        /// <summary>
        /// Gets the View matrix for this camera.
        /// </summary>
        public Matrix4x4 View { get; private set; }

        /// <summary>
        /// Gets the Projection matrix for this camera.
        /// </summary>
        public Matrix4x4 Projection
        {
            get
            {
                EnsureProjection();
                return _projection;
            }
            set
            {
                _projection = value;
                SetViewProjectionDirty();
            }
        }

        public Matrix4x4 ViewProjection
        {
            get
            {
                EnsureViewProjection();
                return _viewProjection;
            }
        }

        public Vector3 Position { get; private set; }

        public void SetLookAt(in Vector3 cameraPosition, in Vector3 cameraTarget, in Vector3 up)
        {
            View = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, up);
            _world = Matrix4x4Utility.Invert(View);
            Position = cameraPosition;

            SetViewProjectionDirty();
        }

        private void EnsureViewProjection()
        {
            if (!_isViewProjectionDirty)
            {
                return;
            }

            _viewProjection = View * Projection;
            _boundingFrustum.Matrix = _viewProjection;

            _isViewProjectionDirty = false;
        }

        protected void SetProjectionDirty()
        {
            _isProjectionDirty = true;
            SetViewProjectionDirty();
        }

        protected void SetViewProjectionDirty()
        {
            _isViewProjectionDirty = true;
        }

        private void EnsureProjection()
        {
            if (!_isProjectionDirty)
            {
                return;
            }

            CreateProjection(out _projection);

            _isProjectionDirty = false;
        }

        protected abstract void CreateProjection(out Matrix4x4 projection);
    }
}
