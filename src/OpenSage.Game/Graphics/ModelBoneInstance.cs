using System.Numerics;

namespace OpenSage.Graphics
{
    public sealed class ModelBoneInstance
    {
        private readonly ModelBone _modelBone;

        /// <summary>
        /// Animated transform, relative to original bone transform.
        /// </summary>
        public Transform AnimatedOffset { get; }

        /// <summary>
        /// Is this bone visible? This property can be animated.
        /// </summary>
        public bool Visible { get; internal set; }

        public Matrix4x4 Matrix =>
            AnimatedOffset.Matrix *
            _modelBone.Transform.Matrix;

        internal ModelBoneInstance(ModelBone modelBone)
        {
            _modelBone = modelBone;

            AnimatedOffset = Transform.CreateIdentity();

            Visible = true;
        }
    }
}
