using System;
using LLGfx;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderItem
    {
        public RenderableComponent Renderable;
        public Effect Effect;
        public EffectPipelineStateHandle PipelineStateHandle;
        public Action<CommandEncoder, Effect, EffectPipelineStateHandle> RenderCallback;
    }
}
