using System.Collections.Generic;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Graphics
{
    internal sealed class ModelMeshPartInstance : DisposableBase
    {
        public readonly ModelMeshPart ModelMeshPart;

        private readonly ConstantBuffer<MeshShaderResources.RenderItemConstantsVS> _renderItemConstantsBufferVS;
        private readonly ResourceSet _renderItemConstantsResourceSet;

        public readonly BeforeRenderDelegate BeforeRenderCallback;
        public readonly BeforeRenderDelegate BeforeRenderCallbackDepth;

        public ModelMeshPartInstance(
            ModelMeshPart modelMeshPart,
            ModelMeshInstance modelMeshInstance,
            AssetLoadContext loadContext)
        {
            ModelMeshPart = modelMeshPart;

            _renderItemConstantsBufferVS = AddDisposable(
                new ConstantBuffer<MeshShaderResources.RenderItemConstantsVS>(
                    loadContext.GraphicsDevice,
                    "RenderItemConstantsVS"));

            _renderItemConstantsResourceSet = AddDisposable(
                loadContext.ShaderResources.Mesh.CreateRenderItemConstantsResourceSet(
                    modelMeshPart.ModelMesh.MeshConstantsBuffer,
                    _renderItemConstantsBufferVS,
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
            if (_renderItemConstantsBufferVS.Value.World != renderItem.World)
            {
                _renderItemConstantsBufferVS.Value.World = renderItem.World;
                _renderItemConstantsBufferVS.Update(cl);
            }

            cl.SetGraphicsResourceSet(3, _renderItemConstantsResourceSet);

            ModelMeshPart.BeforeRender(cl);
        }
    }

    internal sealed class ModelMeshInstance : DisposableBase
    {
        public readonly ModelInstance ModelInstance;

        public readonly List<ModelMeshPartInstance> MeshPartInstances = new();

        public ModelMeshInstance(
            ModelMesh modelMesh,
            ModelInstance modelInstance,
            AssetLoadContext loadContext)
        {
            ModelInstance = modelInstance;

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
    }
}
