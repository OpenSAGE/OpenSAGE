using System.Numerics;
using LLGfx;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class Camera
    {
        private Vector3 _position;
        private Vector3 _target;

        private float _fieldOfView;
        private Viewport _viewport;

        private Matrix4x4 _viewMatrix;
        private bool _viewMatrixDirty;

        private Matrix4x4 _projectionMatrix;
        private bool _projectionMatrixDirty;

        private readonly BoundingFrustum _boundingFrustum;
        private bool _boundingFrustumDirty;

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                _viewMatrixDirty = true;
                _boundingFrustumDirty = true;
            }
        }

        public Vector3 Target
        {
            get => _target;
            set
            {
                _target = value;
                _viewMatrixDirty = true;
                _boundingFrustumDirty = true;
            }
        }

        public Matrix4x4 ViewMatrix
        {
            get
            {
                if (_viewMatrixDirty)
                {
                    _viewMatrix = Matrix4x4.CreateLookAt(
                        _position,
                        _target,
                        Vector3.UnitZ);
                    _viewMatrixDirty = false;
                }
                return _viewMatrix;
            }
        }

        /// <summary>
        /// Field of view in degrees.
        /// </summary>
        public float FieldOfView
        {
            get => _fieldOfView;
            set
            {
                _fieldOfView = value;
                _projectionMatrixDirty = true;
                _boundingFrustumDirty = true;
            }
        }

        public Viewport Viewport
        {
            get => _viewport;
            set
            {
                _viewport = value;
                _projectionMatrixDirty = true;
                _boundingFrustumDirty = true;
            }
        }

        public Matrix4x4 ProjectionMatrix
        {
            get
            {
                if (_projectionMatrixDirty)
                {
                    _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                        (_fieldOfView * MathUtility.Pi / 180),
                        _viewport.AspectRatio,
                        0.1f,
                        5000.0f);
                    _projectionMatrixDirty = false;
                }
                return _projectionMatrix;
            }
        }

        public BoundingFrustum BoundingFrustum
        {
            get
            {
                if (_boundingFrustumDirty)
                {
                    _boundingFrustum.Matrix = ViewMatrix * ProjectionMatrix;
                    _boundingFrustumDirty = false;
                }
                return _boundingFrustum;
            }
        }

        public Camera()
        {
            _boundingFrustum = new BoundingFrustum(Matrix4x4.Identity);

            FieldOfView = 60;
        }

        public Ray ScreenPointToRay(Vector2 screenPoint)
        {
            var view = ViewMatrix;
            var world = Matrix4x4.Identity;

            var x = screenPoint.X;
            var y = screenPoint.Y;

            var pos1 = _viewport.Unproject(new Vector3(x, y, 0), _projectionMatrix, view, world);
            var pos2 = _viewport.Unproject(new Vector3(x, y, 1), _projectionMatrix, view, world);
            var dir = Vector3.Normalize(pos2 - pos1);

            return new Ray(pos1, dir);
        }
    }
}
