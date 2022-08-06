using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshPartInstance : DisposableBase
    {
        private readonly ModelMeshInstance _meshInstance;

        public readonly ModelMeshPart ModelMeshPart;

        public readonly BeforeRenderDelegate BeforeRenderCallback;
        public readonly BeforeRenderDelegate BeforeRenderCallbackDepth;

        public ModelMeshPartInstance(
            ModelMeshPart modelMeshPart,
            ModelMeshInstance modelMeshInstance)
        {
            ModelMeshPart = modelMeshPart;
            _meshInstance = modelMeshInstance;

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
            _meshInstance.OnBeforeRender(cl, renderItem);

            ModelMeshPart.BeforeRender(cl);
        }
    }
}
