using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderList : DisposableBase
    {
        public readonly List<RenderListEffectGroup> Opaque = new List<RenderListEffectGroup>();
        public readonly List<RenderListEffectGroup> Transparent = new List<RenderListEffectGroup>();
        public readonly List<RenderListEffectGroup> Gui = new List<RenderListEffectGroup>();

        public readonly List<RenderItem> RenderItems = new List<RenderItem>();

        public void AddRenderItem(RenderItem renderItem)
        {
            RenderItems.Add(renderItem);

            void addItem(List<RenderListEffectGroup> effectGroups)
            {
                var pipelineStateGroup = GetPipelineStateGroup(effectGroups, renderItem);
                pipelineStateGroup.RenderItems.Add(renderItem);
            }

            var blendEnabled = renderItem.PipelineStateHandle.EffectPipelineState.BlendState.Enabled;
            addItem(blendEnabled ? Transparent : Opaque);
        }

        public void AddGuiRenderItem(RenderItem renderItem)
        {
            RenderItems.Add(renderItem);

            var pipelineStateGroup = GetPipelineStateGroup(Gui, renderItem);
            pipelineStateGroup.RenderItems.Add(renderItem);
        }

        private RenderListPipelineStateGroup GetPipelineStateGroup(List<RenderListEffectGroup> effectGroups, RenderItem renderItem)
        {
            var effectGroup = effectGroups.FirstOrDefault(x => x.Effect == renderItem.Effect);
            if (effectGroup == null)
            {
                effectGroups.Add(effectGroup = new RenderListEffectGroup(renderItem.Effect));
            }

            var pipelineStateGroup = effectGroup.PipelineStateGroups.FirstOrDefault(x => x.PipelineStateHandle == renderItem.PipelineStateHandle);
            if (pipelineStateGroup == null)
            {
                effectGroup.PipelineStateGroups.Add(pipelineStateGroup = new RenderListPipelineStateGroup(renderItem.PipelineStateHandle));
            }

            return pipelineStateGroup;
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
                            RenderItems.Remove(renderItem);
                            pipelineStateGroup.RenderItems.Remove(renderItem);
                        }
                    }
                }
            }

            removeItems(Opaque);
            removeItems(Transparent);

            removeItems(Gui);

            void cleanGroups(List<RenderListEffectGroup> effectGroups)
            {
                var effectGroupsToRemove = new List<RenderListEffectGroup>();
                foreach (var effectGroup in effectGroups)
                {
                    var pipelineStateGroupsToRemove = new List<RenderListPipelineStateGroup>();
                    foreach (var pipelineStateGroup in effectGroup.PipelineStateGroups)
                    {
                        if (pipelineStateGroup.RenderItems.Count == 0)
                        {
                            pipelineStateGroupsToRemove.Add(pipelineStateGroup);
                        }
                    }
                    foreach (var toRemove in pipelineStateGroupsToRemove)
                    {
                        effectGroup.PipelineStateGroups.Remove(toRemove);
                    }

                    if (effectGroup.PipelineStateGroups.Count == 0)
                    {
                        effectGroupsToRemove.Add(effectGroup);
                    }
                }
                foreach (var toRemove in effectGroupsToRemove)
                {
                    effectGroups.Remove(toRemove);
                }
            }

            cleanGroups(Opaque);
            cleanGroups(Transparent);
        }
    }
}
