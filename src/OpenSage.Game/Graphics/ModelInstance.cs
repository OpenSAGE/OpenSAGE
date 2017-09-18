using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Graphics.Util;

namespace OpenSage.Graphics
{
    public sealed class ModelInstance : GraphicsObject
    {
        private readonly Model _model;
        private readonly bool _anySkinnedMeshes;

        private readonly DynamicBuffer _skinningConstantBuffer;
        private SkinningConstants _skinningConstants;

        private Matrix4x4[] _absoluteBoneMatrices;

        public Matrix4x4[] AnimatedBoneTransforms { get; }
        public bool[] AnimatedBoneVisibilities { get; }

        public Model Model => _model;

        public Matrix4x4 WorldMatrix { get; set; } = Matrix4x4.Identity;

        public ModelInstance(Model model, GraphicsDevice graphicsDevice)
        {
            _model = model;
            _anySkinnedMeshes = _model.Meshes.Any(x => x.Skinned);

            _absoluteBoneMatrices = new Matrix4x4[_model.Bones.Length];

            AnimatedBoneTransforms = new Matrix4x4[_model.Bones.Length];
            for (var i = 0; i < _model.Bones.Length; i++)
            {
                AnimatedBoneTransforms[i] = Matrix4x4.Identity;
            }

            AnimatedBoneVisibilities = new bool[_model.Bones.Length];
            for (var i = 0; i < _model.Bones.Length; i++)
            {
                AnimatedBoneVisibilities[i] = true;
            }

            _skinningConstantBuffer = AddDisposable(DynamicBuffer.Create<SkinningConstants>(graphicsDevice));
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct SkinningConstants
        {
            // Array of MaxBones * float4x3
            public fixed float Bones[Model.MaxBones * 12];

            public void CopyFrom(Matrix4x4[] matrices)
            {
                fixed (float* boneArray = Bones)
                {
                    for (var i = 0; i < matrices.Length; i++)
                    {
                        PointerUtil.CopyToMatrix4x3(
                            ref matrices[i],
                            boneArray + (i * 12));
                    }
                }
            }
        }

        public void PreDraw(CommandEncoder commandEncoder)
        {
            for (var i = 0; i < _model.Bones.Length; i++)
            {
                var bone = _model.Bones[i];

                _absoluteBoneMatrices[i] = bone.Parent != null
                    ? AnimatedBoneTransforms[i] * bone.Transform * _absoluteBoneMatrices[bone.Parent.Index]
                    : AnimatedBoneTransforms[i] * bone.Transform;
            }

            if (_anySkinnedMeshes)
            {
                _skinningConstants.CopyFrom(_absoluteBoneMatrices);
                _skinningConstantBuffer.SetData(ref _skinningConstants);
                commandEncoder.SetInlineConstantBuffer(2, _skinningConstantBuffer);
            }
        }

        public void Draw(
            CommandEncoder commandEncoder,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            DrawImpl(commandEncoder, ref view, ref projection, false);
            DrawImpl(commandEncoder, ref view, ref projection, true);
        }

        private void DrawImpl(
            CommandEncoder commandEncoder,
            ref Matrix4x4 view,
            ref Matrix4x4 projection,
            bool alphaBlended)
        {
            foreach (var mesh in _model.Meshes)
            {
                if (!AnimatedBoneVisibilities[mesh.ParentBone.Index])
                {
                    continue;
                }

                var meshWorld = mesh.Skinned
                    ? WorldMatrix
                    : _absoluteBoneMatrices[mesh.ParentBone.Index] * WorldMatrix;

                mesh.SetMatrices(ref meshWorld, ref view, ref projection);

                mesh.Draw(commandEncoder, alphaBlended);
            }
        }
    }
}
