using System.Diagnostics;
using System.Numerics;

namespace OpenSage.Graphics
{
    [DebuggerDisplay("ModelBoneInstance '{Name}'")]
    public sealed class ModelBoneInstance
    {
        private readonly ModelBone _modelBone;
        private bool _isDirty;
        private bool _visible;
        public string Name { get { return _modelBone.Name; } }

        /// <summary>
        /// Animated transform, relative to original bone transform.
        /// </summary>
        public Transform AnimatedOffset { get; }

        /// <summary>
        /// Is this bone visible? This property can be animated.
        /// </summary>
        public bool Visible
        {
            get => _visible;
            internal set
            {
                _visible = value;
                _isDirty = true;
            }
        }

        internal bool IsDirty => _isDirty || AnimatedOffset.IsDirty;

        public Matrix4x4 Matrix =>
            AnimatedOffset.Matrix *
            _modelBone.Transform.Matrix;

        internal ModelBoneInstance(ModelBone modelBone)
        {
            _modelBone = modelBone;

            AnimatedOffset = Transform.CreateIdentity();

            Visible = true;

            _isDirty = true;
        }

        internal void ResetDirty()
        {
            _isDirty = false;
        }
    }
}
