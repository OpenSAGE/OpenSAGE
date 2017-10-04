using System.Collections.Generic;

namespace LLGfx.Effects
{
    public static class EffectPipelineStateFactory
    {
        private static readonly Dictionary<EffectPipelineState, EffectPipelineStateHandle> Handles;

        static EffectPipelineStateFactory()
        {
            Handles = new Dictionary<EffectPipelineState, EffectPipelineStateHandle>();
        }

        public static EffectPipelineStateHandle GetHandle(ref EffectPipelineState state)
        {
            lock (Handles)
            {
                if (!Handles.TryGetValue(state, out var handle))
                {
                    handle = new EffectPipelineStateHandle(state);
                    Handles.Add(state, handle);
                }
                return handle;
            }
        }
    }
}
