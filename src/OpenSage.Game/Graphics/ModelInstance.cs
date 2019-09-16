using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelInstance : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;

        /// <summary>
        /// Bone transforms relative to root bone.
        /// </summary>
        internal readonly Matrix4x4[] RelativeBoneTransforms;

        /// <summary>
        /// Bone transforms in world space.
        /// </summary>
        internal readonly Matrix4x4[] AbsoluteBoneTransforms;

        private Matrix4x4 _worldMatrix;

        private bool _relativeBoneTransformsDirty;

        /// <summary>
        /// Calculated bone visibilities. Child bones will be hidden
        /// if their parent bones are hidden.
        /// </summary>
        internal readonly bool[] BoneVisibilities;

        internal readonly BeforeRenderDelegate[][] BeforeRenderDelegates;
        internal readonly BeforeRenderDelegate[][] BeforeRenderDelegatesDepth;

        private readonly bool _hasSkinnedMeshes;

        private readonly Matrix4x4[] _skinningBones;

        private readonly DeviceBuffer _skinningBuffer;

        internal readonly ResourceSet SkinningBufferResourceSet;

        public readonly Model Model;

        public readonly ModelBoneInstance[] ModelBoneInstances;

        public readonly List<AnimationInstance> AnimationInstances;

        internal ModelInstance(Model model, AssetLoadContext loadContext)
        {
            Model = model;

            _graphicsDevice = loadContext.GraphicsDevice;

            ModelBoneInstances = new ModelBoneInstance[model.BoneHierarchy.Bones.Length];
            for (var i = 0; i < model.BoneHierarchy.Bones.Length; i++)
            {
                ModelBoneInstances[i] = new ModelBoneInstance(model.BoneHierarchy.Bones[i]);
            }

            RelativeBoneTransforms = new Matrix4x4[model.BoneHierarchy.Bones.Length];
            AbsoluteBoneTransforms = new Matrix4x4[model.BoneHierarchy.Bones.Length];

            BoneVisibilities = new bool[model.BoneHierarchy.Bones.Length];
            for (var i = 0; i < model.BoneHierarchy.Bones.Length; i++)
            {
                BoneVisibilities[i] = true;
            }

            _hasSkinnedMeshes = model.SubObjects.Any(x => x.RenderObject.Skinned);

            DeviceBuffer skinningBuffer;
            if (_hasSkinnedMeshes)
            {
                _skinningBuffer = skinningBuffer = AddDisposable(_graphicsDevice.ResourceFactory.CreateBuffer(
                    new BufferDescription(
                        (uint) (64 * model.BoneHierarchy.Bones.Length),
                        BufferUsage.StructuredBufferReadOnly | BufferUsage.Dynamic,
                        64,
                        true)));

                _skinningBones = new Matrix4x4[model.BoneHierarchy.Bones.Length];
            }
            else
            {
                skinningBuffer = loadContext.StandardGraphicsResources.GetNullStructuredBuffer(64);
            }

            SkinningBufferResourceSet = AddDisposable(loadContext.ShaderResources.Mesh.CreateSkinningResourceSet(skinningBuffer));

            AnimationInstances = new List<AnimationInstance>();

            _relativeBoneTransformsDirty = true;

            BeforeRenderDelegates = new BeforeRenderDelegate[model.SubObjects.Length][];
            BeforeRenderDelegatesDepth = new BeforeRenderDelegate[model.SubObjects.Length][];

            for (var i = 0; i < model.SubObjects.Length; i++)
            {
                var mesh = model.SubObjects[i].RenderObject;

                BeforeRenderDelegates[i] = new BeforeRenderDelegate[mesh.MeshParts.Count];
                BeforeRenderDelegatesDepth[i] = new BeforeRenderDelegate[mesh.MeshParts.Count];

                for (var j = 0; j < mesh.MeshParts.Count; j++)
                {
                    var meshBeforeRender = mesh.BeforeRenderDelegates[j];
                    var meshBeforeRenderDepth = mesh.BeforeRenderDelegatesDepth[j];

                    BeforeRenderDelegates[i][j] = (cl, context) =>
                    {
                        cl.SetGraphicsResourceSet(8, SkinningBufferResourceSet);
                        meshBeforeRender(cl, context);
                    };

                    BeforeRenderDelegatesDepth[i][j] = (cl, context) =>
                    {
                        cl.SetGraphicsResourceSet(3, SkinningBufferResourceSet);
                        meshBeforeRenderDepth(cl, context);
                    };
                }
            }
        }

        public void Update(in TimeInterval gameTime)
        {
            // TODO: Don't update animations if model isn't visible.

            // Update animations.
            foreach (var animationInstance in AnimationInstances)
            {
                if (animationInstance.Update(gameTime))
                {
                    _relativeBoneTransformsDirty = true;
                }
            }

            if (_relativeBoneTransformsDirty)
            {
                // Calculate (animated) bone transforms relative to root bone.
                for (var i = 0; i < Model.BoneHierarchy.Bones.Length; i++)
                {
                    var bone = Model.BoneHierarchy.Bones[i];

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
            }

            _relativeBoneTransformsDirty = false;

            if (!_hasSkinnedMeshes)
            {
                return;
            }

            // If the model has skinned meshes, convert relative bone transforms
            // to Matrix4x3 to send to shader.
            for (var i = 0; i < Model.BoneHierarchy.Bones.Length; i++)
            {
                _skinningBones[i] = RelativeBoneTransforms[i];
                //RelativeBoneTransforms[i].ToMatrix4x3(out _skinningBones[i]);
            }

            _graphicsDevice.UpdateBuffer(_skinningBuffer, 0, _skinningBones);
        }

        public void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
            if (worldMatrix == _worldMatrix)
            {
                return;
            }

            _worldMatrix = worldMatrix;

            for (var i = 0; i < Model.BoneHierarchy.Bones.Length; i++)
            {
                AbsoluteBoneTransforms[i] = RelativeBoneTransforms[i] * worldMatrix;
            }
        }

        internal void BuildRenderList(
            RenderList renderList,
            Camera camera,
            bool castsShadow,
            Player owner)
        {
            for (var i = 0; i < Model.SubObjects.Length; i++)
            {
                var subObject = Model.SubObjects[i];

                subObject.RenderObject.BuildRenderList(
                    renderList,
                    camera,
                    this,
                    BeforeRenderDelegates[i],
                    BeforeRenderDelegatesDepth[i],
                    subObject.Bone,
                    _worldMatrix,
                    castsShadow,
                    owner);
            }
        }
    }
}
