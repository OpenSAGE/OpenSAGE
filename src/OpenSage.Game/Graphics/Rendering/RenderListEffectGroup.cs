using System.Collections.Generic;
using LLGfx.Effects;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderListEffectGroup
    {
        public readonly Effect Effect;
        public readonly List<RenderListPipelineStateGroup> PipelineStateGroups = new List<RenderListPipelineStateGroup>();

        public RenderListEffectGroup(Effect effect)
        {
            Effect = effect;
        }
    }
}
