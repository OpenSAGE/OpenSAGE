using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public readonly struct EffectPipelineState
    {
        public readonly RasterizerStateDescription RasterizerState;

        public readonly DepthStencilStateDescription DepthStencilState;

        public readonly BlendStateDescription BlendState;

        public readonly OutputDescription OutputDescription;

        public readonly EffectPipelineStateHandle Handle;

        public EffectPipelineState(
            RasterizerStateDescription rasterizerState,
            DepthStencilStateDescription depthStencilState,
            BlendStateDescription blendState,
            OutputDescription outputDescription)
        {
            RasterizerState = rasterizerState;
            DepthStencilState = depthStencilState;
            BlendState = blendState;

            OutputDescription = outputDescription;

            Handle = null;
            Handle = EffectPipelineStateFactory.GetHandle(this);
        }
    }
}
