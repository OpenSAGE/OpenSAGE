using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderPipeline : DisposableBase
    {
        private readonly DepthStencilBufferCache _depthStencilBufferCache;
        private readonly ConstantBuffer<GlobalConstantsShared> _globalConstantBufferShared;
        private readonly ConstantBuffer<GlobalConstantsVS> _globalConstantBufferVS;
        private readonly ConstantBuffer<GlobalConstantsPS> _globalConstantBufferPS;
        private readonly ConstantBuffer<RenderItemConstantsVS> _renderItemConstantsBufferVS;
        private readonly ConstantBuffer<LightingConstants> _globalLightingTerrainBuffer;
        private readonly ConstantBuffer<LightingConstants> _globalLightingObjectBuffer;

        public RenderPipeline(GraphicsDevice graphicsDevice)
        {
            _depthStencilBufferCache = AddDisposable(new DepthStencilBufferCache(graphicsDevice));

            _globalConstantBufferShared = AddDisposable(new ConstantBuffer<GlobalConstantsShared>(graphicsDevice));
            _globalConstantBufferVS = AddDisposable(new ConstantBuffer<GlobalConstantsVS>(graphicsDevice));
            _renderItemConstantsBufferVS = AddDisposable(new ConstantBuffer<RenderItemConstantsVS>(graphicsDevice));
            _globalConstantBufferPS = AddDisposable(new ConstantBuffer<GlobalConstantsPS>(graphicsDevice));
            _globalLightingTerrainBuffer = AddDisposable(new ConstantBuffer<LightingConstants>(graphicsDevice));
            _globalLightingObjectBuffer = AddDisposable(new ConstantBuffer<LightingConstants>(graphicsDevice));
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

                instanceData.Update(context.GraphicsDevice, context.Camera);
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

            commandEncoder.SetViewport(context.Camera.Viewport);

            SetGlobalConstantBuffers(commandEncoder, context);

            void doDrawPass(List<RenderListEffectGroup> effectGroups)
            {
                foreach (var effectGroup in effectGroups)
                {
                    var effect = effectGroup.Effect;

                    effect.Begin(commandEncoder);

                    var globalConstantsSharedParameter = effect.GetParameter("GlobalConstantsShared", throwIfMissing: false);
                    if (globalConstantsSharedParameter != null)
                    {
                        globalConstantsSharedParameter.SetData(_globalConstantBufferShared.Buffer);
                    }

                    var globalConstantsVSParameter = effect.GetParameter("GlobalConstantsVS", throwIfMissing: false);
                    if (globalConstantsVSParameter != null)
                    {
                        globalConstantsVSParameter.SetData(_globalConstantBufferVS.Buffer);
                    }

                    var globalConstantsPSParameter = effect.GetParameter("GlobalConstantsPS", throwIfMissing: false);
                    if (globalConstantsPSParameter != null)
                    {
                        globalConstantsPSParameter.SetData(_globalConstantBufferPS.Buffer);
                    }

                    var lightingConstantsObjectParameter = effect.GetParameter("LightingConstants_Object", throwIfMissing: false);
                    if (lightingConstantsObjectParameter != null)
                    {
                        lightingConstantsObjectParameter.SetData(_globalLightingObjectBuffer.Buffer);
                    }

                    var lightingConstantsTerrainParameter = effect.GetParameter("LightingConstants_Terrain", throwIfMissing: false);
                    if (lightingConstantsTerrainParameter != null)
                    {
                        lightingConstantsTerrainParameter.SetData(_globalLightingTerrainBuffer.Buffer);
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

                            var renderItemConstantsVSParameter = effect.GetParameter("RenderItemConstantsVS", throwIfMissing: false);
                            if (renderItemConstantsVSParameter != null)
                            {
                                _renderItemConstantsBufferVS.Value.World = renderItem.Renderable.Entity.Transform.LocalToWorldMatrix;
                                _renderItemConstantsBufferVS.Update();
                                renderItemConstantsVSParameter.SetData(_renderItemConstantsBufferVS.Buffer);
                            }

                            renderItem.Material.Apply();

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

            doDrawPass(renderList.Gui);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(context.SwapChain);
        }

        private void SetGlobalConstantBuffers(CommandEncoder commandEncoder, RenderContext context)
        {
            var cameraPosition = Matrix4x4Utility.Invert(context.Camera.View).Translation;

            _globalConstantBufferShared.Value.CameraPosition = cameraPosition;
            _globalConstantBufferShared.Update();

            _globalConstantBufferVS.Value.ViewProjection = context.Camera.View * context.Camera.Projection;
            _globalConstantBufferVS.Update();

            _globalConstantBufferPS.Value.TimeInSeconds = (float) context.GameTime.TotalGameTime.TotalSeconds;
            _globalConstantBufferPS.Value.ViewportSize = context.Camera.Viewport.Size;
            _globalConstantBufferPS.Update();

            _globalLightingTerrainBuffer.Value = context.Scene.Settings.CurrentLightingConfiguration.TerrainLights;
            _globalLightingObjectBuffer.Value = context.Scene.Settings.CurrentLightingConfiguration.ObjectLights;
            _globalLightingTerrainBuffer.Update();
            _globalLightingObjectBuffer.Update();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GlobalConstantsShared
        {
            public Vector3 CameraPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GlobalConstantsVS
        {
            public Matrix4x4 ViewProjection;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RenderItemConstantsVS
        {
            public Matrix4x4 World;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GlobalConstantsPS
        {
            public float TimeInSeconds;
            public Vector2 ViewportSize;
        }
    }
}
