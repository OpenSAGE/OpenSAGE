using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Terrain
{
    public sealed class RoadMaterial : EffectMaterial
    {
        public RoadMaterial(ContentManager contentManager, Effect effect)
            : base(contentManager, effect)
        {
            SetProperty("Sampler", contentManager.GraphicsDevice.Aniso4xSampler);

            PipelineState = new EffectPipelineState(
                RasterizerStateDescriptionUtility.CullNoneSolid, // TODO
                DepthStencilStateDescription.DepthOnlyLessEqualRead,
                BlendStateDescription.SingleAlphaBlend,
                RenderPipeline.GameOutputDescription);
        }

        public void SetTexture(Texture texture)
        {
            SetProperty("Texture", texture);
        }
    }
}
