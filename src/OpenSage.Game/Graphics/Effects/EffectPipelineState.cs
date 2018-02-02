using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public struct EffectPipelineState
    {
        private EffectPipelineStateHandle _handle;

        public RasterizerStateDescription RasterizerState { get; }

        public DepthStencilStateDescription DepthStencilState { get; }

        public BlendStateDescription BlendState { get; }

        public OutputDescription OutputDescription { get; }

        public EffectPipelineState(
            RasterizerStateDescription rasterizerState,
            DepthStencilStateDescription depthStencilState,
            BlendStateDescription blendState,
            OutputDescription outputDescription = default) // TODO: Ugly.
        {
            RasterizerState = rasterizerState;
            DepthStencilState = depthStencilState;
            BlendState = blendState;

            OutputDescription = outputDescription;

            // TODO: This is a bit ugly.
            _handle = null;
            _handle = EffectPipelineStateFactory.GetHandle(ref this);
        }

        public EffectPipelineStateHandle GetHandle(in OutputDescription outputDescription)
        {
            var clone = new EffectPipelineState(
                RasterizerState,
                DepthStencilState,
                BlendState,
                outputDescription);

            return clone._handle;
        }
    }
}
