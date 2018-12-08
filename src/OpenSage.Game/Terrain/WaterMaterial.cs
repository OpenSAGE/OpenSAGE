using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Terrain
{
    public sealed class WaterMaterial : EffectMaterial
    {
        public override LightingType LightingType => LightingType.Terrain;

        public WaterMaterial(ContentManager contentManager, Effect effect)
            : base(contentManager, effect)
        {
            SetProperty("Sampler", contentManager.GraphicsDevice.Aniso4xSampler);

            PipelineState = new EffectPipelineState(
                RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                DepthStencilStateDescription.DepthOnlyLessEqualRead,
                BlendStateDescription.SingleAlphaBlend,
                RenderPipeline.GameOutputDescription);
        }

        public void SetWaterTexture(Texture texture)
        {
            SetProperty("WaterTexture", texture);
        }
    }
}
