using System;
using System.Numerics;
using LLGfx;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras
{
    public abstract class CameraComponent : EntityComponent
    {
        private readonly BoundingFrustum _frustum;
        private readonly RenderContext _renderContext;

        private RectangleF _normalizedViewportRectangle;
        private float _nearPlaneDistance;
        private float _farPlaneDistance;
        private Size _windowSize;

        private RenderPipeline _renderPipeline;

        private Viewport? _viewport;

        private Matrix4x4? _cachedProjectionMatrix;
        private int _cachedProjectionMatrixWidth;
        private int _cachedProjectionMatrixHeight;

        private bool _boundingFrustumDirty = true;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        protected CameraComponent()
        {
            _frustum = new BoundingFrustum(Matrix4x4.Identity);
            _normalizedViewportRectangle = new RectangleF(0, 0, 1, 1);
            NearPlaneDistance = 0.125f;
            FarPlaneDistance = 5000.0f;
            _windowSize = new Size(100, 100);

            _renderContext = new RenderContext();
        }

        /// <summary>
        /// Gets the position, in world space, of this camera.
        /// </summary>
        public Vector3 Position => View.Translation;

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
        /// Gets the distance from the camera of the camera's near clip plane. 
        /// Used for terrain detail level calculation.
        /// </summary>
        internal float ProjectionNear => NearPlaneDistance;

        /// <summary>
        /// Gets the distance from the camera of the camera's top clip plane. 
        /// Used for terrain detail level calculation.
        /// </summary>
        internal float ProjectionTop => 0.5f;

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

        /// <summary>
        /// Gets or sets whether shadows will be rendered for this camera.
        /// </summary>
        public bool RenderShadows { get; set; } = true;

        /// <summary>
        /// Gets or sets the normalized viewport rectangle for this camera. For example, this can 
        /// be used to implement split-screen or a rear view mirror for an in-car view.
        /// </summary>
        public RectangleF NormalizedViewportRectangle
        {
            get { return _normalizedViewportRectangle; }
            set
            {
                _normalizedViewportRectangle = value;
                ClearCachedViewport();
            }
        }

        /// <summary>
        /// Gets the View matrix for this camera.
        /// </summary>
        public Matrix4x4 View => GetViewMatrix();

        /// <summary>
        /// Gets the Projection matrix for this camera.
        /// </summary>
        public Matrix4x4 Projection
        {
            get
            {
                var pixelWidth = Game.SwapChain.BackBufferWidth;
                var pixelHeight = Game.SwapChain.BackBufferHeight;

                if (_cachedProjectionMatrix != null && (pixelWidth != _cachedProjectionMatrixWidth || pixelHeight != _cachedProjectionMatrixHeight))
                {
                    _cachedProjectionMatrix = null;
                }

                if (_cachedProjectionMatrix == null)
                {
                    _cachedProjectionMatrix = GetProjectionMatrix(Viewport.AspectRatio);

                    _cachedProjectionMatrixWidth = pixelWidth;
                    _cachedProjectionMatrixHeight = pixelHeight;
                }

                return _cachedProjectionMatrix.Value;
            }
        }

        /// <summary>
        /// Gets the rectangle covered by this camera in screen space.
        /// </summary>
        public Rectangle PixelRect => Viewport.Bounds();

        /// <summary>
        /// Gets the width of the rectangle covered by this camera in screen space.
        /// </summary>
        public int PixelWidth => PixelRect.Width;

        /// <summary>
        /// Gets the height of the rectangle covered by this camera in screen space.
        /// </summary>
        public int PixelHeight => PixelRect.Height;

        /// <summary>
        /// Render target to render this camera into. If this is <code>null</code>,
        /// the camera will be rendered into the back buffer and displayed on the screen.
        /// If this is not <code>null</code>, the camera will be rendered into the
        /// specified <see cref="RenderTarget2D"/> and not displayed on the screen.
        /// </summary>
        public RenderTarget RenderTarget { get; set; }

        /// <summary>
        /// Gets or sets how the render target (either backbuffer or texture)
        /// will be cleared prior to rendering.
        /// </summary>
        public CameraClearType ClearType { get; set; } = CameraClearType.DepthAndColor;

        /// <summary>
        /// Gets or sets the culling mask for this camera.
        /// </summary>
        /// <remarks>
        /// The culling mask is a bit mask that is compared against <see cref="GameEntity.Layer"/>
        /// to determine whether a given <see cref="GameEntity"/> should be rendered by this camera.
        /// </remarks>
        public int CullingMask { get; set; } = int.MaxValue;

        /// <summary>
        /// Gets or sets the background color for this camera. Only used when
        /// <see cref="ClearType"/> is set to <see cref="CameraClearType.DepthAndColor"/>.
        /// </summary>
        public ColorRgba BackgroundColor { get; set; } = ColorRgba.DimGray;

        /// <summary>
        /// Gets the viewport for this camera, which defines the area on the screen
        /// that the camera will project into.
        /// </summary>
        public Viewport Viewport
        {
            get
            {
                var pixelWidth = Game.SwapChain.BackBufferWidth;
                var pixelHeight = Game.SwapChain.BackBufferHeight;

                if (_viewport != null && 
                    (pixelWidth != _viewport.Value.Width || pixelHeight != _viewport.Value.Height))
                {
                    _viewport = null;
                }

                if (_viewport == null)
                {
                    var bounds = new Rectangle(
                        (int) (NormalizedViewportRectangle.X * pixelWidth),
                        (int) (NormalizedViewportRectangle.Y * pixelHeight),
                        (int) (NormalizedViewportRectangle.Width * pixelWidth),
                        (int) (NormalizedViewportRectangle.Height * pixelHeight));

                    var maximumViewportBounds = new Rectangle(0, 0, pixelWidth, pixelHeight);

                    var viewportBounds = Rectangle.Intersect(maximumViewportBounds, bounds);

                    _viewport = new Viewport(
                        viewportBounds.X,
                        viewportBounds.Y,
                        viewportBounds.Width,
                        viewportBounds.Height);
                }

                return _viewport.Value;
            }
        }

        private Vector3 _lookDirection;
        public Vector3 LookDirection
        {
            get { return _lookDirection; }
            set
            {
                _lookDirection = value;
                UpdateTransform();
            }
        }

        private float _pitch = 1;
        public float Pitch
        {
            get { return _pitch; }
            set
            {
                _pitch = value;
                UpdateTransform();
            }
        }

        private float _zoom = 1;
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                UpdateTransform();
            }
        }

        private Vector3 _worldPosition;
        public Vector3 WorldPosition
        {
            get { return _worldPosition; }
            set
            {
                _worldPosition = value;
                _targetPosition = null;
                UpdateTransform();
            }
        }

        private Vector3? _targetPosition;
        public Vector3? TargetPosition
        {
            get { return _targetPosition; }
            set
            {
                _targetPosition = value;
                UpdateTransform();
            }
        }

        private void UpdateTransform()
        {
            var defaultPitch = MathUtility.ToRadians(ContentManager.IniDataContext.GameData.CameraPitch);

            var yaw = MathUtility.Atan2(_lookDirection.Y, _lookDirection.X);

            var pitch = MathUtility.Lerp(
                0,
                -defaultPitch,
                _pitch);

            var lookDirection = new Vector3(
                MathUtility.Cos(yaw),
                MathUtility.Sin(yaw),
                MathUtility.Sin(pitch));

            lookDirection = Vector3.Normalize(lookDirection);

            Vector3 newPosition, targetPosition;

            if (_targetPosition != null)
            {
                var cameraHeight = MathUtility.Lerp(
                    0,
                    ContentManager.IniDataContext.GameData.CameraHeight,
                    _zoom);

                // Back up camera from camera "position".
                var toCameraRay = new Ray(_targetPosition.Value, -lookDirection);
                var plane = Plane.CreateFromVertices(
                    new Vector3(0, 0, cameraHeight),
                    new Vector3(0, 1, cameraHeight),
                    new Vector3(1, 0, cameraHeight));
                var toCameraIntersectionDistance = toCameraRay.Intersects(ref plane).Value;
                newPosition = _targetPosition.Value - lookDirection * toCameraIntersectionDistance;
                targetPosition = _targetPosition.Value;
            }
            else
            {
                var cameraHeight = MathUtility.Lerp(
                    Scene.HeightMap.GetHeight(_worldPosition.X, _worldPosition.Y),
                    ContentManager.IniDataContext.GameData.CameraHeight,
                    _zoom);

                newPosition = _worldPosition;
                newPosition.Z = cameraHeight;

                targetPosition = newPosition + lookDirection;
            }

            Transform.LocalPosition = newPosition;
            Transform.LookAt(targetPosition);

            _boundingFrustumDirty = true;
        }

        private void ClearCachedViewport()
        {
            _viewport = null;
            ClearCachedProjectionMatrix();
        }

        /// <summary>
        /// Clears the cached projection matrix. This method should be called
        /// after changes to any properties that affect the projection matrix.
        /// </summary>
        protected void ClearCachedProjectionMatrix()
        {
            _cachedProjectionMatrix = null;
            _boundingFrustumDirty = true;
        }

        /// <inheritdoc />
        protected override void Start()
        {
            _renderPipeline = new RenderPipeline(GraphicsDevice);
        }

        protected override void Destroy()
        {
            _renderPipeline.Dispose();
            _renderPipeline = null;

            base.Destroy();
        }

        internal void Render(GameTime gameTime)
        {
            _renderContext.Game = Game;
            _renderContext.GraphicsDevice = GraphicsDevice;
            _renderContext.Graphics = Graphics;
            _renderContext.Camera = this;
            _renderContext.Scene = Entity.Scene;
            _renderContext.SwapChain = Game.SwapChain;
            _renderContext.RenderTarget = Game.SwapChain.GetNextRenderTarget();
            _renderContext.GameTime = gameTime;

            _renderPipeline.Execute(_renderContext);
        }

        /// <summary>
        /// Gets the projection matrix for this camera.
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio.</param>
        /// <returns>Computed projection matrix.</returns>
        protected abstract Matrix4x4 GetProjectionMatrix(float aspectRatio);

        /// <summary>
        /// Gets the view matrix for this camera.
        /// </summary>
        /// <returns>Computed view matrix.</returns>
        protected virtual Matrix4x4 GetViewMatrix()
        {
            return Entity.Transform.WorldToLocalMatrix;
        }

        /// <summary>
        /// Creates a ray going from the camera's near plane, through the specified screen position, to the camera's far plane.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Ray ScreenPointToRay(Vector2 position)
        {
            var near = Viewport.Unproject(new Vector3(position, 0), Projection, View, Matrix4x4.Identity);
            var far = Viewport.Unproject(new Vector3(position, 1), Projection, View, Matrix4x4.Identity);

            return new Ray(near, Vector3.Normalize(far - near));
        }

        /// <summary>
        /// Converts a world-space point to screen-space.
        /// </summary>
        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            return Viewport.Project(position, Projection, View, Matrix4x4.Identity);
        }

        internal Rectangle? WorldToScreenRectangle(Vector3 position, Size screenSize)
        {
            var screenPosition = WorldToScreenPoint(position);

            // Check if point is behind camera, or too far away.
            if (screenPosition.Z < Viewport.MinDepth || screenPosition.Z > Viewport.MaxDepth)
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
