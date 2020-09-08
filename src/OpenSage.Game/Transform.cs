using System;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage
{
    public sealed class Transform
    {
        public static Transform CreateIdentity()
        {
            return new Transform(Vector3.Zero, Quaternion.Identity);
        }

        private readonly Vector4 _lookDirBase = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);

        private Vector3 _translation;
        private Quaternion _rotation;
        private float _scale;

        private Matrix4x4 _matrix;
        private bool _isMatrixDirty = true;

        private Matrix4x4 _matrixInverse;
        private bool _isMatrixInverseDirty = true;

        public Vector3 Translation
        {
            get => _translation;
            set
            {
                _translation = value;
                SetMatricesDirty();
            }
        }

        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                SetMatricesDirty();
                LookDirection = Vector4.Transform(_lookDirBase, _rotation).ToVector3();
            }
        }

        public float Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                SetMatricesDirty();
            }
        }

        internal bool IsDirty => _isMatrixDirty || _isMatrixInverseDirty;

        private void SetMatricesDirty()
        {
            _isMatrixDirty = true;
            _isMatrixInverseDirty = true;
        }

        public Transform(in Vector3 translation, in Quaternion rotation, float scale = 1f)
        {
            Translation = translation;
            Rotation = rotation;
            Scale = scale;
        }

        public Transform(in Matrix4x4 matrix)
        {
            Matrix = matrix;
        }

        public Vector3 LookDirection { get; private set; }

        public Matrix4x4 Matrix
        {
            get
            {
                if (_isMatrixDirty)
                {
                    _matrix =
                        Matrix4x4.CreateScale(Scale) *
                        Matrix4x4.CreateFromQuaternion(Rotation) *
                        Matrix4x4.CreateTranslation(Translation);
                    _isMatrixDirty = false;
                }
                return _matrix;
            }

            internal set
            {
                _matrix = value;

                if (!Matrix4x4.Decompose(
                    value,
                    out var scale,
                    out var rotation,
                    out var translation))
                {
                    throw new InvalidOperationException();
                }

                // We assume uniform scale.

                Scale = scale.Z;
                Rotation = rotation;
                Translation = translation;

                _isMatrixDirty = false;
                _isMatrixInverseDirty = true;
            }
        }

        public Matrix4x4 MatrixInverse
        {
            get
            {
                if (_isMatrixInverseDirty)
                {
                    _matrixInverse = Matrix4x4Utility.Invert(Matrix);
                    _isMatrixInverseDirty = false;
                }
                return _matrixInverse;
            }
        }
        public Vector3 EulerAngles
        {
            get
            {
                var x = MathF.Atan2(Matrix.M32, Matrix.M33);
                var y = MathF.Atan2(-Matrix.M31, MathF.Sqrt(MathF.Pow(Matrix.M32, 2) + MathF.Pow(Matrix.M33, 2)));
                var z = MathF.Atan2(Matrix.M21, Matrix.M11);
                return new Vector3(x, y, z);
            }
        }
    }
}
