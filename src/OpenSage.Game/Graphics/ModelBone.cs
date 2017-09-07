using System.Numerics;

namespace OpenSage.Graphics
{
    public sealed class ModelBone
    {
        public int Index { get; }

        public string Name { get; }

        public ModelBone Parent { get; }

        private Vector3 _translation;
        public Vector3 Translation
        {
            get => _translation;
            set
            {
                _translation = value;
                _cachedTransform = null;
            }
        }

        private Quaternion _rotation;
        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _cachedTransform = null;
            }
        }

        private Matrix4x4? _cachedTransform;
        public Matrix4x4 Transform
        {
            get
            {
                if (_cachedTransform == null)
                {
                    _cachedTransform = Matrix4x4.CreateFromQuaternion(_rotation) * Matrix4x4.CreateTranslation(_translation);
                }
                return _cachedTransform.Value;
            }
        }

        public ModelBoneCollection Children { get; internal set; }

        internal ModelBone(int index, string name, ModelBone parent, Vector3 translation, Quaternion rotation)
        {
            Index = index;
            Name = name;
            Parent = parent;

            _translation = translation;
            _rotation = rotation;
        }
    }
}
