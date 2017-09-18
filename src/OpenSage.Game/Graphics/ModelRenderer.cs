using LLGfx;
using OpenSage.Content;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics
{
    public sealed class ModelRenderer : GraphicsObject
    {
        private readonly ContentManager _contentManager;

        private readonly DynamicBuffer _lightingConstantBuffer;

        public ModelRenderer(ContentManager contentManager)
        {
            _contentManager = contentManager;

            _lightingConstantBuffer = AddDisposable(DynamicBuffer.Create<LightingConstants>(contentManager.GraphicsDevice));
        }

        public void PreDrawModels(
            CommandEncoder commandEncoder, 
            ref LightingConstants lightingConstants)
        {
            _contentManager.ModelEffect.Apply(commandEncoder);

            _lightingConstantBuffer.SetData(ref lightingConstants);
            commandEncoder.SetInlineConstantBuffer(3, _lightingConstantBuffer);
        }
    }
}
