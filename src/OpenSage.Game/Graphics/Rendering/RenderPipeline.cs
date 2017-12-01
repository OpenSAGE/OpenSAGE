using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderPipeline : DisposableBase
    {
        private readonly DepthStencilBufferCache _depthStencilBufferCache;
        private readonly Buffer<GlobalConstantsVS> _globalConstantBufferVS;
        private readonly Buffer<RenderItemConstantsVS> _renderItemConstantsBufferVS;
        private readonly Buffer<GlobalConstantsPS> _globalConstantBufferPS;
        private readonly Buffer<LightingConstants> _globalLightingTerrainBuffer;
        private readonly Buffer<LightingConstants> _globalLightingObjectBuffer;
        private GlobalConstantsVS _globalConstantsVS;
        private RenderItemConstantsVS _renderItemConstantsVS;
        private GlobalConstantsPS _globalConstantsPS;
        private LightingConstants _globalLightingTerrain;
        private LightingConstants _globalLightingObject;

        public RenderPipeline(GraphicsDevice graphicsDevice)
        {
            _depthStencilBufferCache = AddDisposable(new DepthStencilBufferCache(graphicsDevice));

            _globalConstantBufferVS = Buffer<GlobalConstantsVS>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer);
            _renderItemConstantsBufferVS = Buffer<RenderItemConstantsVS>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer);
            _globalConstantBufferPS = Buffer<GlobalConstantsPS>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer);
            _globalLightingTerrainBuffer = Buffer<LightingConstants>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer);
            _globalLightingObjectBuffer = Buffer<LightingConstants>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer);
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

                    var lightingConstantsObjectParameter = effect.GetParameter("LightingConstants_Object");
                    if (lightingConstantsObjectParameter != null)
                    {
                        lightingConstantsObjectParameter.SetData(_globalLightingObjectBuffer);
                    }

                    var lightingConstantsTerrainParameter = effect.GetParameter("LightingConstants_Terrain");
                    if (lightingConstantsTerrainParameter != null)
                    {
                        lightingConstantsTerrainParameter.SetData(_globalLightingTerrainBuffer);
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

                            var renderItemConstantsVSParameter = effect.GetParameter("RenderItemConstantsVS");
                            if (renderItemConstantsVSParameter != null)
                            {
                                _renderItemConstantsVS.World = renderItem.Renderable.Entity.Transform.LocalToWorldMatrix;
                                _renderItemConstantsBufferVS.SetData(ref _renderItemConstantsVS);
                                renderItemConstantsVSParameter.SetData(_renderItemConstantsBufferVS);
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

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(context.SwapChain);
        }

        private void SetGlobalConstantBuffers(CommandEncoder commandEncoder, RenderContext context)
        {
            var cameraPosition = Matrix4x4Utility.Invert(context.Camera.View).Translation;

            _globalConstantsVS.ViewProjection = context.Camera.View * context.Camera.Projection;
            _globalConstantsVS.CameraPosition = cameraPosition;
            _globalConstantBufferVS.SetData(ref _globalConstantsVS);
            commandEncoder.SetVertexShaderConstantBuffer(0, _globalConstantBufferVS);

            _globalConstantsPS.CameraPosition = cameraPosition;
            _globalConstantsPS.TimeInSeconds = (float) context.GameTime.TotalGameTime.TotalSeconds;
            _globalConstantsPS.ViewportSize = context.Camera.Viewport.Size;
            _globalConstantBufferPS.SetData(ref _globalConstantsPS);
            commandEncoder.SetPixelShaderConstantBuffer(0, _globalConstantBufferPS);

            _globalLightingTerrain = context.Scene.Settings.CurrentLightingConfiguration.TerrainLights;
            _globalLightingObject = context.Scene.Settings.CurrentLightingConfiguration.ObjectLights;
            _globalLightingTerrainBuffer.SetData(ref _globalLightingTerrain);
            _globalLightingObjectBuffer.SetData(ref _globalLightingObject);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GlobalConstantsVS
        {
            public Matrix4x4 ViewProjection;
            public Vector3 CameraPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RenderItemConstantsVS
        {
            public Matrix4x4 World;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GlobalConstantsPS
        {
            public Vector3 CameraPosition;
            public float TimeInSeconds;
            public Vector2 ViewportSize;
        }
    }
}
