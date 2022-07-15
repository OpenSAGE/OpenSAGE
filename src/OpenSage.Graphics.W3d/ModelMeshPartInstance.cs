using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshPartInstance : DisposableBase
    {
        private readonly ResourceSet _renderItemConstantsResourceSet;

        public readonly ModelMeshPart ModelMeshPart;
        public readonly ModelMeshInstance MeshInstance;

        public readonly BeforeRenderDelegate BeforeRenderCallback;
        public readonly BeforeRenderDelegate BeforeRenderCallbackDepth;

        public ModelMeshPartInstance(
            ModelMeshPart modelMeshPart,
            ModelMeshInstance modelMeshInstance,
            MeshShaderResources meshShaderResources)
        {
            ModelMeshPart = modelMeshPart;
            MeshInstance = modelMeshInstance;

            _renderItemConstantsResourceSet = AddDisposable(
                meshShaderResources.CreateRenderItemConstantsResourceSet(
                    modelMeshPart.ModelMesh.MeshConstantsBuffer,
                    modelMeshInstance.RenderItemConstantsBufferVS,
                    modelMeshInstance.ModelInstance.SkinningBuffer,
                    modelMeshInstance.ModelInstance.RenderItemConstantsBufferPS));

            BeforeRenderCallback = (CommandList cl, in RenderItem renderItem) =>
            {
                OnBeforeRender(cl,  renderItem);
            };

            BeforeRenderCallbackDepth = (CommandList cl, in RenderItem renderItem) =>
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
}
