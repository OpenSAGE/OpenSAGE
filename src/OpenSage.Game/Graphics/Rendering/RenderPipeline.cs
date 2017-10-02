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

            var commandBuffer = context.GraphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            commandEncoder.SetViewport(context.Camera.Viewport);

            // TODO: Object lights.
            var lights = context.Scene.Settings.CurrentLightingConfiguration.TerrainLights;

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
                            if (!renderItem.Renderable.Entity.VisibleInHierarchy)
                            {
                                continue;
                            }

                            // TODO: Test this.
                            //if (!context.Camera.BoundingFrustum.Intersects(renderItem.Renderable.BoundingBox))
                            //{
                            //    continue;
                            //}

                            if (effect is IEffectMatrices m2)
                            {
                                m2.SetWorld(renderItem.Renderable.Entity.Transform.LocalToWorldMatrix);
                            }

                            renderItem.RenderCallback(
                                commandEncoder,
                                renderItem.Effect,
                                renderItem.PipelineStateHandle);
                        }
                    }
                }
            }

            // TODO: Culling, based on:
            // - Renderable.BoundingBox
            // - Renderable.VisibleInHierarchy
            // - Renderable.IsAlwaysVisible

            var renderList = context.Graphics.RenderList;

            doDrawPass(renderList.Opaque);
            doDrawPass(renderList.Transparent);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(context.SwapChain);
        }
    }
}
