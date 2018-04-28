using OpenSage.Content;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class MeshDepthMaterial : MeshMaterial
    {
        public MeshDepthMaterial(ContentManager contentManager, Effect effect)
            : base(contentManager, effect)
        {
            var rasterizerState = RasterizerStateDescription.Default;
            rasterizerState.DepthClipEnabled = false;
            rasterizerState.ScissorTestEnabled = false;

            PipelineState = new EffectPipelineState(
                rasterizerState,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                BlendStateDescription.SingleDisabled,
                RenderPipeline.DepthPassDescription);
        }
    }
}
