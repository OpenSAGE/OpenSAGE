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
    }
}
