using System.Linq;
using System.Numerics;
using LLGfx;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics
{
    public sealed class ModelInstance : GraphicsObject
    {
        private readonly Model _model;
        private readonly bool _anySkinnedMeshes;

        private Matrix4x4[] _absoluteBoneMatrices;

        public Matrix4x4[] AnimatedBoneTransforms { get; }
        public bool[] AnimatedBoneVisibilities { get; }

        public Model Model => _model;

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
        }

        public void Draw(
            CommandEncoder commandEncoder,
            MeshEffect meshEffect,
            ref Matrix4x4 world,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
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
                meshEffect.SetAbsoluteBoneTransforms(_absoluteBoneMatrices);
            }

            DrawImpl(
                commandEncoder,
                meshEffect,
                ref world,
                ref view,
                ref projection,
                false);

            DrawImpl(
                commandEncoder,
                meshEffect,
                ref world,
                ref view,
                ref projection,
                true);
        }

        private void DrawImpl(
            CommandEncoder commandEncoder,
            MeshEffect meshEffect,
            ref Matrix4x4 world,
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
                    ? world
                    : _absoluteBoneMatrices[mesh.ParentBone.Index] * world;

                mesh.Draw(
                    commandEncoder,
                    meshEffect,
                    ref meshWorld,
                    ref view,
                    ref projection,
                    alphaBlended);
            }
        }
    }
}
