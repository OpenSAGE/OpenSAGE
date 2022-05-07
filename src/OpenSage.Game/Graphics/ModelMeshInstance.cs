using System.Collections.Generic;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Graphics
{
    internal sealed class ModelMeshPartInstance : DisposableBase
    {
        private readonly ResourceSet _renderItemConstantsResourceSet;

        public readonly ModelMeshPart ModelMeshPart;
        public readonly ModelMeshInstance MeshInstance;

        public readonly BeforeRenderDelegate BeforeRenderCallback;
        public readonly BeforeRenderDelegate BeforeRenderCallbackDepth;

        public ModelMeshPartInstance(
            ModelMeshPart modelMeshPart,
            ModelMeshInstance modelMeshInstance,
            AssetLoadContext loadContext)
        {
            ModelMeshPart = modelMeshPart;
            MeshInstance = modelMeshInstance;

            _renderItemConstantsResourceSet = AddDisposable(
                loadContext.ShaderResources.Mesh.CreateRenderItemConstantsResourceSet(
                    modelMeshPart.ModelMesh.MeshConstantsBuffer,
                    modelMeshInstance.RenderItemConstantsBufferVS,
                    modelMeshInstance.ModelInstance.SkinningBuffer,
                    modelMeshInstance.ModelInstance.RenderItemConstantsBufferPS));

            BeforeRenderCallback = (CommandList cl, RenderContext context, in RenderItem renderItem) =>
            {
                OnBeforeRender(cl,  renderItem);
            };

            BeforeRenderCallbackDepth = (CommandList cl, RenderContext context, in RenderItem renderItem) =>
            {
                OnBeforeRender(cl, renderItem);
            };
        }

        private void OnBeforeRender(
            CommandList cl,
            in RenderItem renderItem)
        {
            MeshInstance.OnBeforeRender(cl, renderItem);

            cl.SetGraphicsResourceSet(3, _renderItemConstantsResourceSet);

            ModelMeshPart.BeforeRender(cl);
        }
    }

    internal sealed class ModelMeshInstance : DisposableBase
    {
        internal readonly ConstantBuffer<MeshShaderResources.RenderItemConstantsVS> RenderItemConstantsBufferVS;

        public readonly ModelInstance ModelInstance;

        public readonly List<ModelMeshPartInstance> MeshPartInstances = new();

        public ModelMeshInstance(
            ModelMesh modelMesh,
            ModelInstance modelInstance,
            AssetLoadContext loadContext)
        {
            ModelInstance = modelInstance;

            RenderItemConstantsBufferVS = AddDisposable(
                new ConstantBuffer<MeshShaderResources.RenderItemConstantsVS>(
                    loadContext.GraphicsDevice,
                    $"{modelMesh.SubObject.FullName}_RenderItemConstantsVS"));

            foreach (var modelMeshPart in modelMesh.MeshParts)
            {
                MeshPartInstances.Add(
                    AddDisposable(
                        new ModelMeshPartInstance(
                            modelMeshPart,
                            this,
                            loadContext)));
            }
        }

        internal void OnBeforeRender(
            CommandList cl,
            in RenderItem renderItem)
        {
            if (RenderItemConstantsBufferVS.Value.World != renderItem.World)
            {
                RenderItemConstantsBufferVS.Value.World = renderItem.World;
                RenderItemConstantsBufferVS.Update(cl);
            }
        }
    }
}
