using System.Collections.Generic;
using LLGfx;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderPipeline : DisposableBase
    {
        private readonly DepthStencilBufferCache _depthStencilBufferCache;

        public RenderPipeline(GraphicsDevice graphicsDevice)
        {
            _depthStencilBufferCache = AddDisposable(new DepthStencilBufferCache(graphicsDevice));
        }

        public void Execute(RenderContext context)
        {
            var renderList = context.Graphics.RenderList;

            // Culling
            foreach (var renderItem in renderList.RenderItems)
            {
                renderItem.Visible = true;

                if (renderItem.Renderable.IsAlwaysVisible)
                {
                    continue;
                }

                if (!renderItem.Renderable.Entity.VisibleInHierarchy)
                {
                    renderItem.Visible = false;
                    continue;
                }

                if (!context.Camera.BoundingFrustum.Intersects(renderItem.Renderable.BoundingBox))
                {
                    renderItem.Visible = false;
                    continue;
                }
            }

            foreach (var instanceData in renderList.InstanceData.Values)
            {
                foreach (var instancedRenderable in instanceData.InstancedRenderables)
                {
                    instancedRenderable.Visible = true;

                    if (instancedRenderable.Renderable.IsAlwaysVisible)
                    {
                        continue;
                    }

                    if (!instancedRenderable.Renderable.Entity.VisibleInHierarchy)
                    {
                        instancedRenderable.Visible = false;
                        continue;
                    }

                    if (!context.Camera.BoundingFrustum.Intersects(instancedRenderable.Renderable.BoundingBox))
                    {
                        instancedRenderable.Visible = false;
                        continue;
                    }
                }

                instanceData.Update(context.GraphicsDevice);
            }

            var commandBuffer = context.GraphicsDevice.CommandQueue.GetCommandBuffer();

            var renderPassDescriptor = new RenderPassDescriptor();

            var clearColor = context.Camera.BackgroundColor.ToColorRgba();

            renderPassDescriptor.SetRenderTargetDescriptor(
                context.RenderTarget,
                LoadAction.Clear,
                clearColor);

            var depthStencilBuffer = _depthStencilBufferCache.Get(
                context.SwapChain.BackBufferWidth,
                context.SwapChain.BackBufferHeight);

            renderPassDescriptor.SetDepthStencilDescriptor(depthStencilBuffer);

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            // TODO: Object lights.
            var lights = context.Scene.Settings.CurrentLightingConfiguration.TerrainLights;

            commandEncoder.SetViewport(context.Camera.Viewport);

            void doDrawPass(List<RenderListEffectGroup> effectGroups)
            {
                foreach (var effectGroup in effectGroups)
                {
                    var effect = effectGroup.Effect;

                    effect.Begin(commandEncoder);

                    if (effect is IEffectMatrices m)
                    {
                        m.SetView(context.Camera.View);
                        m.SetProjection(context.Camera.Projection);
                    }

                    if (effect is IEffectLights l)
                    {
                        l.SetLights(ref lights);
                    }

                    if (effect is IEffectTime t)
                    {
                        t.SetTimeInSeconds(context.GameTime.TotalGameTime.Seconds);
                    }

                    foreach (var pipelineStateGroup in effectGroup.PipelineStateGroups)
                    {
                        var pipelineStateHandle = pipelineStateGroup.PipelineStateHandle;
                        effect.SetPipelineState(pipelineStateHandle);

                        foreach (var renderItem in pipelineStateGroup.RenderItems)
                        {
                            if (!renderItem.Visible)
                            {
                                continue;
                            }

                            if (effect is IEffectMatrices m2)
                            {
                                m2.SetWorld(renderItem.Renderable.Entity.Transform.LocalToWorldMatrix);
                            }

                            renderItem.RenderCallback(
                                commandEncoder,
                                effectGroup.Effect,
                                pipelineStateGroup.PipelineStateHandle,
                                null);
                        }

                        foreach (var instancedRenderItem in pipelineStateGroup.InstancedRenderItems)
                        {
                            if (instancedRenderItem.InstanceData.NumInstances == 0)
                            {
                                continue;
                            }

                            instancedRenderItem.RenderCallback(
                                commandEncoder,
                                effectGroup.Effect,
                                pipelineStateGroup.PipelineStateHandle,
                                instancedRenderItem.InstanceData);
                        }
                    }
                }
            }

            // TODO: Culling, based on:
            // - Renderable.BoundingBox
            // - Renderable.VisibleInHierarchy
            // - Renderable.IsAlwaysVisible

            doDrawPass(renderList.Opaque);
            doDrawPass(renderList.Transparent);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(context.SwapChain);
        }
    }
}
