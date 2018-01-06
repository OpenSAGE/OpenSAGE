using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class ModelComponent : EntityComponent
    {
        private readonly bool _hasSkinnedMeshes;

        private Matrix4x3[] _skinningBones;

        internal Buffer<Matrix4x3> SkinningBuffer;

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
                SkinningBuffer = Buffer<Matrix4x3>.CreateDynamicArray(
                    GraphicsDevice,
                    Bones.Length,
                    BufferBindFlags.ShaderResource);

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

            SkinningBuffer.SetData(_skinningBones);
        }
    }
}
