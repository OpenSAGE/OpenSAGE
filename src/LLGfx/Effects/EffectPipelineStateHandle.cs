namespace LLGfx.Effects
{
    public sealed class EffectPipelineStateHandle
    {
        public EffectPipelineState EffectPipelineState;

        internal EffectPipelineStateHandle(EffectPipelineState effectPipelineState)
        {
            EffectPipelineState = effectPipelineState;
        }
    }
}
