using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
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
        internal readonly Matrix4x4[] RelativeBoneTransforms;

        /// <summary>
        /// Bone transforms in world space.
        /// </summary>
        internal readonly Matrix4x4[] AbsoluteBoneTransforms;

        private Matrix4x4 _worldMatrix;

        /// <summary>
        /// Calculated bone visibilities. Child bones will be hidden
        /// if their parent bones are hidden.
        /// </summary>
        internal readonly bool[] BoneVisibilities;

        /// <summary>
        /// Calculated bone visibilities for the current frame. Child bones will be hidden
        /// if their parent bones are hidden.
        /// </summary>
        internal readonly bool[] BoneFrameVisibilities;

        internal readonly BeforeRenderDelegate[][] BeforeRenderDelegates;
        internal readonly BeforeRenderDelegate[][] BeforeRenderDelegatesDepth;

        private readonly Matrix4x4[] _skinningBones;

        private readonly DeviceBuffer _skinningBuffer;

        public readonly Model Model;

        public readonly ModelBoneInstance[] ModelBoneInstances;

        public readonly List<AnimationInstance> AnimationInstances;

        // TODO: Use this.
        public ColorRgba HouseColor;

        public readonly bool[] UnknownBools;

        private readonly ConstantBuffer<MeshShaderResources.RenderItemConstantsPS> _renderItemConstantsBufferPS;

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
            BoneFrameVisibilities = new bool[model.BoneHierarchy.Bones.Length];

            for (var i = 0; i < model.BoneHierarchy.Bones.Length; i++)
            {
                BoneVisibilities[i] = true;
                BoneFrameVisibilities[i] = true;
            }

            DeviceBuffer skinningBuffer;
            if (model.HasSkinnedMeshes)
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

            AnimationInstances = new List<AnimationInstance>();

            _renderItemConstantsBufferPS = AddDisposable(new ConstantBuffer<MeshShaderResources.RenderItemConstantsPS>(_graphicsDevice, "RenderItemConstantsPS"));

            BeforeRenderDelegates = new BeforeRenderDelegate[model.SubObjects.Length][];
            BeforeRenderDelegatesDepth = new BeforeRenderDelegate[model.SubObjects.Length][];

            for (var i = 0; i < model.SubObjects.Length; i++)
            {
                var renderObject = model.SubObjects[i].RenderObject;

                if (!(renderObject is ModelMesh mesh))
                {
                    continue;
                }

                var renderItemConstantsBufferVS = AddDisposable(new ConstantBuffer<MeshShaderResources.RenderItemConstantsVS>(_graphicsDevice, "RenderItemConstantsVS"));

                var renderItemConstantsResourceSet = AddDisposable(
                    loadContext.ShaderResources.Mesh.CreateRenderItemConstantsResourceSet(
                        mesh.MeshConstantsBuffer,
                        renderItemConstantsBufferVS,
                        skinningBuffer,
                        _renderItemConstantsBufferPS));

                BeforeRenderDelegates[i] = new BeforeRenderDelegate[mesh.MeshParts.Count];
                BeforeRenderDelegatesDepth[i] = new BeforeRenderDelegate[mesh.MeshParts.Count];

                for (var j = 0; j < mesh.MeshParts.Count; j++)
                {
                    var meshBeforeRender = mesh.BeforeRenderDelegates[j];
                    var meshBeforeRenderDepth = mesh.BeforeRenderDelegatesDepth[j];

                    BeforeRenderDelegates[i][j] = (CommandList cl, RenderContext context, in RenderItem renderItem) =>
                    {
                        if (renderItemConstantsBufferVS.Value.World != renderItem.World)
                        {
                            renderItemConstantsBufferVS.Value.World = renderItem.World;
                            renderItemConstantsBufferVS.Update(_graphicsDevice);
                        }

                        cl.SetGraphicsResourceSet(3, renderItemConstantsResourceSet);
                        meshBeforeRender(cl, context, renderItem);
                    };

                    BeforeRenderDelegatesDepth[i][j] = (CommandList cl, RenderContext context, in RenderItem renderItem) =>
                    {
                        if (renderItemConstantsBufferVS.Value.World != renderItem.World)
                        {
                            renderItemConstantsBufferVS.Value.World = renderItem.World;
                            renderItemConstantsBufferVS.Update(_graphicsDevice);
                        }

                        cl.SetGraphicsResourceSet(3, renderItemConstantsResourceSet);
                        meshBeforeRenderDepth(cl, context, renderItem);
                    };
                }
            }

            UnknownBools = new bool[model.SubObjects.Length];
        }

        public void Update(in TimeInterval gameTime)
        {
            // TODO: Don't update animations if model isn't visible.

            // Update animations.
            foreach (var animationInstance in AnimationInstances)
            {
                animationInstance.Update(gameTime);
            }

            // Check if any model bone transform has changed.
            var isAnyModelBoneInstanceDirty = false;
            foreach (var modelBoneInstance in ModelBoneInstances)
            {
                if (modelBoneInstance.IsDirty)
                {
                    isAnyModelBoneInstanceDirty = true;
                    break;
                }
            }

            if (isAnyModelBoneInstanceDirty)
            {
                // Calculate (animated) bone transforms relative to root bone.
                for (var i = 0; i < Model.BoneHierarchy.Bones.Length; i++)
                {
                    var bone = Model.BoneHierarchy.Bones[i];

                    var parentTransform = bone.Parent != null
                        ? RelativeBoneTransforms[bone.Parent.Index]
                        : Matrix4x4.Identity;

                    RelativeBoneTransforms[i] = ModelBoneInstances[i].Matrix * parentTransform;

                    var parentVisible = bone.Parent == null || BoneVisibilities[bone.Parent.Index];

                    BoneFrameVisibilities[i] = BoneVisibilities[i] && parentVisible && ModelBoneInstances[i].Visible;
                }
            }

            foreach (var modelBoneInstance in ModelBoneInstances)
            {
                modelBoneInstance.ResetDirty();
            }

            if (!Model.HasSkinnedMeshes)
            {
                return;
            }

            for (var i = 0; i < Model.BoneHierarchy.Bones.Length; i++)
            {
                _skinningBones[i] = RelativeBoneTransforms[i];
            }

            _graphicsDevice.UpdateBuffer(_skinningBuffer, 0, _skinningBones);
        }

        public ref readonly Matrix4x4 GetWorldMatrix() => ref _worldMatrix;

        public void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
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
            MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS,
            Dictionary<string, bool> shownSubObjects = null,
            Dictionary<string, bool> hiddenSubObjects = null)
        {
            if (_renderItemConstantsBufferPS.Value != renderItemConstantsPS)
            {
                _renderItemConstantsBufferPS.Value = renderItemConstantsPS;
                _renderItemConstantsBufferPS.Update(_graphicsDevice);
            }

            for (var i = 0; i < Model.SubObjects.Length; i++)
            {
                var subObject = Model.SubObjects[i];
                var name = subObject.Name;

                if ((subObject.RenderObject.Hidden && !(shownSubObjects?.ContainsKey(name) ?? false))
                    || (hiddenSubObjects?.ContainsKey(name) ?? false))
                {
                    continue;
                }

                subObject.RenderObject.BuildRenderList(
                    renderList,
                    camera,
                    this,
                    BeforeRenderDelegates[i],
                    BeforeRenderDelegatesDepth[i],
                    subObject.Bone,
                    _worldMatrix,
                    castsShadow,
                    renderItemConstantsPS);
            }
        }
    }
}
