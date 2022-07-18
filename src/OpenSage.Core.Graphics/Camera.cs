using System;
using System.Numerics;
using OpenSage.Mathematics;
using Viewport = Veldrid.Viewport;

namespace OpenSage.Graphics.Cameras
{
    public sealed class Camera
    {
        private readonly Func<Viewport> _getViewport;
        private Viewport _viewport;

        private float _nearPlaneDistance;
        private float _farPlaneDistance;

        private Matrix4x4 _world, _projection;

        private float _fieldOfView = MathUtility.ToRadians(50);

        /// <summary>
        /// Gets or sets a value that represents the camera's field of view in radians. 
        /// </summary>
        public float FieldOfView
        {
            get { return _fieldOfView; }
            set
            {
                _fieldOfView = value;
                UpdateProjection();
            }
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
        public Vector3 Target { get; private set; }
        public Vector3 Up { get; private set; }

        public Camera(Func<Viewport> getViewport)
        {
            _getViewport = getViewport;
            _viewport = getViewport();

            View = Matrix4x4.Identity;
            BoundingFrustum = new BoundingFrustum(Matrix4x4.Identity);

            _nearPlaneDistance = 4.0f;
            _farPlaneDistance = 10000.0f;

            UpdateProjection();
        }

        public void SetMirrorX(float pivot)
        {
            // Used for rendering reflection without stencil clipping
            var position = Position - new Vector3(0, 0, 2 * (Position.Z - pivot));
            var target = Target - new Vector3(0, 0, 2 * (Target.Z - pivot));
            var up = Up - new Vector3(0, 0, 2 * Up.Z);

            SetLookAt(position, target, up);
            ViewProjection *= Matrix4x4.CreateScale(-1, 1, 1);
            BoundingFrustum.Matrix = ViewProjection;
        }

        public void SetLookAt(in Vector3 cameraPosition, in Vector3 cameraTarget, in Vector3 up)
        {
            View = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, up);
            _world = Matrix4x4Utility.Invert(View);
            Position = cameraPosition;
            Target = cameraTarget;
            Up = up;

            UpdateViewProjection();
        }

        private void UpdateViewProjection()
        {
            ViewProjection = View * _projection;
            BoundingFrustum.Matrix = ViewProjection;
        }

        private void UpdateProjection()
        {
            CreateProjection(out _projection);
            UpdateViewProjection();
        }

        private float GetVerticalFieldOfView() => _fieldOfView / (_viewport.Width / _viewport.Height);

        private void CreateProjection(out Matrix4x4 projection)
        {
            projection = Matrix4x4.CreatePerspectiveFieldOfView(
                GetVerticalFieldOfView(), _viewport.Width / _viewport.Height,
                NearPlaneDistance, FarPlaneDistance);
        }

        /// <summary>
        /// Creates a ray going from the camera's near plane, through the specified screen position, to the camera's far plane.
        /// </summary>
        public Ray ScreenPointToRay(Vector2 position)
        {
            var near = _viewport.Unproject(new Vector3(position, 0), Projection, View, Matrix4x4.Identity);
            var far = _viewport.Unproject(new Vector3(position, 1), Projection, View, Matrix4x4.Identity);

            return new Ray(near, Vector3.Normalize(far - near));
        }

        /// <summary>
        /// Converts a world-space point to screen-space.
        /// </summary>
        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            return _viewport.Project(position, Projection, View, Matrix4x4.Identity);
        }

        public RectangleF? WorldToScreenRectangle(in Vector3 position, in SizeF screenSize)
        {
            var screenPosition = WorldToScreenPoint(position);

            // Check if point is behind camera, or too far away.
            if (!IsWithinViewportDepth(screenPosition))
            {
                return null;
            }

            return new RectangleF(
                screenPosition.X - screenSize.Width / 2.0f,
                screenPosition.Y - screenSize.Height / 2.0f,
                screenSize.Width, screenSize.Height);
        }

        public bool IsWithinViewportDepth(in Vector3 screenPosition)
        {
            return screenPosition.Z >= _viewport.MinDepth && screenPosition.Z <= _viewport.MaxDepth;
        }

        /// <summary>
        /// Converts a screen-space point to world-space. The value of <paramref name="position.Z"/>
        /// will be used to determine the depth in camera space of the world-space point.
        /// </summary>
        public Vector3 ScreenToWorldPoint(in Vector3 position)
        {
            var ray = ScreenPointToRay(new Vector2(position.X, position.Y));
            return ray.Position + ray.Direction * position.Z;
        }

        public void OnViewportSizeChanged()
        {
            _viewport = _getViewport();
            UpdateProjection();
        }

        /// <summary>
        /// Get size in screen space of world-space bounding sphere.
        /// </summary>
        public float GetScreenSize(in BoundingSphere boundingSphere)
        {
            var distance = Vector3.Distance(boundingSphere.Center, Position);
            return (boundingSphere.Radius / (MathF.Tan(GetVerticalFieldOfView() / 2) * distance)) * (_viewport.Height / 2);
        }

        /// <summary>
        /// Converts the bounding box from world space into a screen space bounding rectangle.
        /// </summary>
        public Rectangle GetBoundingRectangle(in AxisAlignedBoundingBox boundingBox)
        {
            var topLeft = new Vector3(float.MaxValue);
            var bottomRight = new Vector3(float.MinValue);

            Span<Vector3> vertices = stackalloc Vector3[8];

            var min = boundingBox.Min;
            var max = boundingBox.Max;

            // Bottom plane
            vertices[0] = min;
            vertices[1] = min.WithX(max.X);
            vertices[2] = min.WithY(max.Y);
            vertices[3] = max.WithZ(min.Z);

            // Top plane
            vertices[4] = max;
            vertices[5] = max.WithX(min.X);
            vertices[6] = max.WithY(min.Y);
            vertices[7] = min.WithZ(max.Z);

            for (var i = 0; i < 8; i++)
            {
                var screenPos = WorldToScreenPoint(vertices[i]);
                topLeft = Vector3.Min(topLeft, screenPos);
                bottomRight = Vector3.Max(bottomRight, screenPos);
            }

            var size = bottomRight - topLeft;
            return new Rectangle((int) topLeft.X, (int) topLeft.Y, (int) size.X, (int) size.Y);
        }
    }
}
