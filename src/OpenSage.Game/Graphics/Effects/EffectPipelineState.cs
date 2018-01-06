using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Graphics.Effects
{
    public struct EffectPipelineState
    {
        private EffectPipelineStateHandle _handle;

        public RasterizerStateDescription RasterizerState { get; }

        public DepthStencilStateDescription DepthStencilState { get; }

        public BlendStateDescription BlendState { get; }

        public EffectPipelineState(
            RasterizerStateDescription rasterizerState,
            DepthStencilStateDescription depthStencilState,
            BlendStateDescription blendState)
        {
            RasterizerState = rasterizerState;
            DepthStencilState = depthStencilState;
            BlendState = blendState;

            // TODO: This is a bit ugly.
            _handle = null;
            _handle = EffectPipelineStateFactory.GetHandle(ref this);
        }

        public EffectPipelineStateHandle GetHandle() => _handle;
    }
}
