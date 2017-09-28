using System.Collections.Generic;
using System.Linq;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderList
    {
        public readonly List<RenderListEffectGroup> Opaque = new List<RenderListEffectGroup>();
        public readonly List<RenderListEffectGroup> Transparent = new List<RenderListEffectGroup>();

        public void AddRenderItem(RenderItem renderItem)
        {
            void addItem(List<RenderListEffectGroup> effectGroups)
            {
                var effectGroup = effectGroups.FirstOrDefault(x => x.Effect == renderItem.Effect);
                if (effectGroup == null)
                {
                    effectGroups.Add(effectGroup = new RenderListEffectGroup
                    {
                        Effect = renderItem.Effect
                    });
                }

                var pipelineStateGroup = effectGroup.PipelineStateGroups.FirstOrDefault(x => x.PipelineStateHandle == renderItem.PipelineStateHandle);
                if (pipelineStateGroup == null)
                {
                    effectGroup.PipelineStateGroups.Add(pipelineStateGroup = new RenderListPipelineStateGroup
                    {
                        PipelineStateHandle = renderItem.PipelineStateHandle
                    });
                }

                pipelineStateGroup.RenderItems.Add(renderItem);
            }

            var blendEnabled = renderItem.PipelineStateHandle.EffectPipelineState.BlendState.Enabled;
            addItem(blendEnabled ? Transparent : Opaque);
        }

        public void RemoveRenderable(RenderableComponent renderable)
        {
            void removeItems(List<RenderListEffectGroup> effectGroups)
            {
                foreach (var effectGroup in effectGroups)
                {
                    foreach (var pipelineStateGroup in effectGroup.PipelineStateGroups)
                    {
                        var toRemove = new List<RenderItem>();
                        foreach (var renderItem in pipelineStateGroup.RenderItems)
                        {
                            if (renderItem.Renderable == renderable)
                            {
                                toRemove.Add(renderItem);
                            }
                        }
                        foreach (var renderItem in toRemove)
                        {
                            pipelineStateGroup.RenderItems.Remove(renderItem);
                        }
                    }
                }
            }

            removeItems(Opaque);
            removeItems(Transparent);
        }
    }

    internal sealed class RenderListEffectGroup
    {
        public Effect Effect;
        public readonly List<RenderListPipelineStateGroup> PipelineStateGroups = new List<RenderListPipelineStateGroup>();
    }

    internal sealed class RenderListPipelineStateGroup
    {
        public EffectPipelineStateHandle PipelineStateHandle;
        public readonly List<RenderItem> RenderItems = new List<RenderItem>();
    }
}
