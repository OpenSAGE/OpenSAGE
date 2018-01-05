using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderPipeline : DisposableBase
    {
        private readonly RenderList _renderList;

        private readonly DepthStencilBufferCache _depthStencilBufferCache;
        private readonly ConstantBuffer<GlobalConstantsShared> _globalConstantBufferShared;
        private readonly ConstantBuffer<GlobalConstantsVS> _globalConstantBufferVS;
        private readonly ConstantBuffer<GlobalConstantsPS> _globalConstantBufferPS;
        private readonly ConstantBuffer<RenderItemConstantsVS> _renderItemConstantsBufferVS;
        private readonly ConstantBuffer<LightingConstants> _globalLightingTerrainBuffer;
        private readonly ConstantBuffer<LightingConstants> _globalLightingObjectBuffer;

        public RenderPipeline(GraphicsDevice graphicsDevice)
        {
            _renderList = new RenderList();

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
            _renderList.Clear();

            context.Scene.Scene3D?.World.Terrain.BuildRenderList(_renderList);

            foreach (var system in context.Game.GameSystems)
            {
                system.BuildRenderList(_renderList);
            }

            var commandBuffer = context.GraphicsDevice.CommandQueue.GetCommandBuffer();

            var renderPassDescriptor = new RenderPassDescriptor();

            var clearColor = context.Camera.BackgroundColor.ToColorRgbaF();

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

            UpdateGlobalConstantBuffers(commandEncoder, context);

            void doRenderPass(RenderBucket bucket)
            {
                Culler.Cull(bucket.RenderItems, bucket.CulledItems, context);

                if (bucket.CulledItems.Count == 0)
                {
                    return;
                }

                bucket.CulledItems.Sort();

                RenderItem? lastRenderItem = null;
                foreach (var renderItem in bucket.CulledItems)
                {
                    if (lastRenderItem == null || lastRenderItem.Value.Effect != renderItem.Effect)
                    {
                        var effect = renderItem.Effect;

                        effect.Begin(commandEncoder);

                        SetDefaultConstantBuffers(effect);
                    }

                    if (lastRenderItem == null || lastRenderItem.Value.VertexBuffer0 != renderItem.VertexBuffer0)
                    {
                        commandEncoder.SetVertexBuffer(0, renderItem.VertexBuffer0);
                    }

                    if (lastRenderItem == null || lastRenderItem.Value.VertexBuffer1 != renderItem.VertexBuffer1)
                    {
                        commandEncoder.SetVertexBuffer(1, renderItem.VertexBuffer1);
                    }

                    var renderItemConstantsVSParameter = renderItem.Effect.GetParameter("RenderItemConstantsVS", throwIfMissing: false);
                    if (renderItemConstantsVSParameter != null)
                    {
                        _renderItemConstantsBufferVS.Value.World = renderItem.World;
                        _renderItemConstantsBufferVS.Update();
                        renderItemConstantsVSParameter.SetData(_renderItemConstantsBufferVS.Buffer);
                    }

                    renderItem.Material.Apply();

                    renderItem.Effect.Apply(commandEncoder);

                    switch (renderItem.DrawCommand)
                    {
                        case DrawCommand.Draw:
                            commandEncoder.Draw(
                                PrimitiveType.TriangleList,
                                renderItem.VertexStart,
                                renderItem.VertexCount);
                            break;

                        case DrawCommand.DrawIndexed:
                            commandEncoder.DrawIndexed(
                                PrimitiveType.TriangleList,
                                renderItem.IndexCount,
                                renderItem.IndexBuffer,
                                renderItem.StartIndex);
                            break;

                        default:
                            throw new System.Exception();
                    }

                    lastRenderItem = renderItem;
                }
            }

            doRenderPass(_renderList.Opaque);
            doRenderPass(_renderList.Transparent);

            doRenderPass(_renderList.Gui);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(context.SwapChain);
        }

        private void SetDefaultConstantBuffers(Effect effect)
        {
            void setDefaultConstantBuffer(string name, Buffer buffer)
            {
                var parameter = effect.GetParameter(name, throwIfMissing: false);
                if (parameter != null)
                {
                    parameter.SetData(buffer);
                }
            }

            setDefaultConstantBuffer("GlobalConstantsShared", _globalConstantBufferShared.Buffer);
            setDefaultConstantBuffer("GlobalConstantsVS", _globalConstantBufferVS.Buffer);
            setDefaultConstantBuffer("GlobalConstantsPS", _globalConstantBufferPS.Buffer);
            setDefaultConstantBuffer("LightingConstants_Object", _globalLightingObjectBuffer.Buffer);
            setDefaultConstantBuffer("LightingConstants_Terrain", _globalLightingTerrainBuffer.Buffer);
        }

        private void UpdateGlobalConstantBuffers(CommandEncoder commandEncoder, RenderContext context)
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
