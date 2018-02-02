using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelComponent : EntityComponent
    {
        private readonly bool _hasSkinnedMeshes;

        private Matrix4x3[] _skinningBones;

        internal DeviceBuffer SkinningBuffer;

        public TransformComponent[] Bones { get; }

        internal ModelComponent(
            TransformComponent[] bones,
            bool hasSkinnedMeshes)
        {
            Bones = bones;

            _hasSkinnedMeshes = hasSkinnedMeshes;
        }

        protected override void Start()
        {
            base.Start();

            if (_hasSkinnedMeshes)
            {
                SkinningBuffer = GraphicsDevice.ResourceFactory.CreateBuffer(
                    new BufferDescription(
                        (uint) (48 * Bones.Length),
                        BufferUsage.StructuredBufferReadOnly | BufferUsage.Dynamic));

                _skinningBones = new Matrix4x3[Bones.Length];
            }
        }

        protected override void Destroy()
        {
            SkinningBuffer?.Dispose();

            base.Destroy();
        }

        internal void UpdateBoneTransforms()
        {
            if (!_hasSkinnedMeshes)
            {
                return;
            }

            for (var i = 0; i < Bones.Length; i++)
            {
                // Bone matrix should be relative to root bone transform.
                var rootBoneMatrix = Bones[0].LocalToWorldMatrix;
                var boneMatrix = Bones[i].LocalToWorldMatrix;

                var boneMatrixRelativeToRoot = boneMatrix * Matrix4x4Utility.Invert(rootBoneMatrix);

                boneMatrixRelativeToRoot.ToMatrix4x3(out _skinningBones[i]);
            }

            GraphicsDevice.UpdateBuffer(SkinningBuffer, 0, _skinningBones);
        }
    }
}
