using System.Numerics;

namespace OpenSage.Graphics
{
    public sealed class ModelComponent : EntityComponent
    {
        private TransformComponent[] _bones;

        public TransformComponent[] Bones
        {
            get { return _bones; }
            set
            {
                _bones = value;
                AbsoluteBoneTransforms = new Matrix4x4[value.Length];
            }
        }

        internal Matrix4x4[] AbsoluteBoneTransforms;

        internal void UpdateAbsoluteBoneTransforms()
        {
            for (var i = 0; i < Bones.Length; i++)
            {
                AbsoluteBoneTransforms[i] = Bones[i].LocalToWorldMatrix;
            }
        }
    }
}
