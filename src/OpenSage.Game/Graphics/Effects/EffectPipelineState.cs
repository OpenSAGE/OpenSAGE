using System.Collections.Generic;
using LLGfx;

namespace OpenSage.Graphics.Effects
{
    public struct EffectPipelineState
    {
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
        }

        public EffectPipelineStateHandle GetHandle()
        {
            return EffectPipelineStateFactory.GetHandle(ref this);
        }
    }

    public sealed class EffectPipelineStateHandle
    {
        internal EffectPipelineState EffectPipelineState;

        internal EffectPipelineStateHandle(EffectPipelineState effectPipelineState)
        {
            EffectPipelineState = effectPipelineState;
        }
    }

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
