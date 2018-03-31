using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelInstance : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;

        /// <summary>
        /// Bone transforms relative to root bone.
        /// </summary>
        internal Matrix4x4[] RelativeBoneTransforms;

        /// <summary>
        /// Bone transforms in world space.
        /// </summary>
        internal Matrix4x4[] AbsoluteBoneTransforms;

        private Matrix4x4 _worldMatrix;

        /// <summary>
        /// Calculated bone visibilities. Child bones will be hidden
        /// if their parent bones are hidden.
        /// </summary>
        internal bool[] BoneVisibilities;

        private readonly bool _hasSkinnedMeshes;

        private readonly Matrix4x4[] _skinningBones;

        internal DeviceBuffer SkinningBuffer;

        public Model Model { get; }

        public ModelBoneInstance[] ModelBoneInstances { get; }

        public List<AnimationInstance> AnimationInstances { get; }

        internal ModelInstance(Model model, GraphicsDevice graphicsDevice)
        {
            Model = model;

            _graphicsDevice = graphicsDevice;

            ModelBoneInstances = new ModelBoneInstance[model.Bones.Length];
            for (var i = 0; i < model.Bones.Length; i++)
            {
                ModelBoneInstances[i] = new ModelBoneInstance(model.Bones[i]);
            }

            RelativeBoneTransforms = new Matrix4x4[model.Bones.Length];
            AbsoluteBoneTransforms = new Matrix4x4[model.Bones.Length];

            BoneVisibilities = new bool[model.Bones.Length];
            for (var i = 0; i < model.Bones.Length; i++)
            {
                BoneVisibilities[i] = true;
            }

            _hasSkinnedMeshes = model.Meshes.Any(x => x.Skinned);

            if (_hasSkinnedMeshes)
            {
                SkinningBuffer = AddDisposable(graphicsDevice.ResourceFactory.CreateBuffer(
                    new BufferDescription(
                        (uint) (64 * model.Bones.Length),
                        BufferUsage.StructuredBufferReadOnly | BufferUsage.Dynamic,
                        64)));

                _skinningBones = new Matrix4x4[model.Bones.Length];
            }

            AnimationInstances = new List<AnimationInstance>();
            foreach (var animation in model.Animations)
            {
                AnimationInstances.Add(new AnimationInstance(this, animation));
            }
        }

        public void Update(GameTime gameTime)
        {
            // Update animations.
            foreach (var animationInstance in AnimationInstances)
            {
                animationInstance.Update(gameTime);
            }

            // Calculate (animated) bone transforms relative to root bone.
            for (var i = 0; i < Model.Bones.Length; i++)
            {
                var bone = Model.Bones[i];

                var parentTransform = bone.Parent != null
                    ? RelativeBoneTransforms[bone.Parent.Index]
                    : Matrix4x4.Identity;

                RelativeBoneTransforms[i] =
                    ModelBoneInstances[i].Matrix *
                    parentTransform;

                var parentVisible = bone.Parent != null
                    ? BoneVisibilities[i]
                    : true;

                BoneVisibilities[i] = parentVisible && ModelBoneInstances[i].Visible;
            }

            if (!_hasSkinnedMeshes)
            {
                return;
            }

            // If the model has skinned meshes, convert relative bone transforms
            // to Matrix4x3 to send to shader.
            for (var i = 0; i < Model.Bones.Length; i++)
            {
                _skinningBones[i] = RelativeBoneTransforms[i];
                //RelativeBoneTransforms[i].ToMatrix4x3(out _skinningBones[i]);
            }

            _graphicsDevice.UpdateBuffer(SkinningBuffer, 0, _skinningBones);
        }

        public void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
            _worldMatrix = worldMatrix;

            for (var i = 0; i < Model.Bones.Length; i++)
            {
                AbsoluteBoneTransforms[i] = RelativeBoneTransforms[i] * worldMatrix;
            }
        }

        public void BuildRenderList(
            RenderList renderList,
            CameraComponent camera)
        {
            foreach (var mesh in Model.Meshes)
            {
                mesh.BuildRenderList(renderList, camera, this, _worldMatrix);
            }
        }
    }
}
