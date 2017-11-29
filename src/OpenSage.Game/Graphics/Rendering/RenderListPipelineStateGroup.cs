using System.Collections.Generic;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderListPipelineStateGroup
    {
        public readonly EffectPipelineStateHandle PipelineStateHandle;
        public readonly List<RenderItem> RenderItems = new List<RenderItem>();
        public readonly List<InstancedRenderItem> InstancedRenderItems = new List<InstancedRenderItem>();

        public RenderListPipelineStateGroup(EffectPipelineStateHandle pipelineStateHandle)
        {
            PipelineStateHandle = pipelineStateHandle;
        }
    }
}
