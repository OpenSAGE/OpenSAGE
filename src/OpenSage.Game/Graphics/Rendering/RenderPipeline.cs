using System;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.Map;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering.Shadows;
using OpenSage.Gui;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderPipeline : DisposableBase
    {
        public static readonly OutputDescription GameOutputDescription = new OutputDescription(
            new OutputAttachmentDescription(PixelFormat.D24_UNorm_S8_UInt),
            new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));

        private readonly RenderList _renderList;

        private readonly CommandList _commandList;

        private readonly ConstantBuffer<GlobalConstantsShared> _globalConstantBufferShared;
        private readonly ConstantBuffer<GlobalConstantsVS> _globalConstantBufferVS;
        private readonly ConstantBuffer<GlobalConstantsPS> _globalConstantBufferPS;
        private readonly ConstantBuffer<RenderItemConstantsVS> _renderItemConstantsBufferVS;
        private readonly ConstantBuffer<LightingConstantsVS> _globalLightingVSTerrainBuffer;
        private readonly ConstantBuffer<LightingConstantsPS> _globalLightingPSTerrainBuffer;
        private readonly ConstantBuffer<LightingConstantsVS> _globalLightingVSObjectBuffer;
        private readonly ConstantBuffer<LightingConstantsPS> _globalLightingPSObjectBuffer;
        private readonly ConstantBuffer<ShadowConstantsPS> _shadowConstantsPSBuffer;

        private readonly DrawingContext2D _drawingContext;

        private readonly Sampler _shadowSampler;

        private readonly ShadowMapRenderer _shadowMapRenderer;

        private ShadowConstantsPS _shadowConstants;

        private Texture _intermediateDepthBuffer;
        private Texture _intermediateTexture;
        private Framebuffer _intermediateFramebuffer;

        private readonly SpriteBatch _intermediateSpriteBatch;

        public Texture ShadowMap => _shadowMapRenderer.ShadowMap;

        public RenderPipeline(Game game)
        {
            _renderList = new RenderList();

            var graphicsDevice = game.GraphicsDevice;

            _globalConstantBufferShared = AddDisposable(new ConstantBuffer<GlobalConstantsShared>(graphicsDevice, "GlobalConstantsShared"));
            _globalConstantBufferVS = AddDisposable(new ConstantBuffer<GlobalConstantsVS>(graphicsDevice, "GlobalConstantsVS"));
            _renderItemConstantsBufferVS = AddDisposable(new ConstantBuffer<RenderItemConstantsVS>(graphicsDevice, "RenderItemConstantsVS"));
            _globalConstantBufferPS = AddDisposable(new ConstantBuffer<GlobalConstantsPS>(graphicsDevice, "GlobalConstantsPS"));
            _globalLightingVSTerrainBuffer = AddDisposable(new ConstantBuffer<LightingConstantsVS>(graphicsDevice, "GlobalLightingConstantsVS (terrain)"));
            _globalLightingPSTerrainBuffer = AddDisposable(new ConstantBuffer<LightingConstantsPS>(graphicsDevice, "GlobalLightingConstantsPS (terrain)"));
            _globalLightingVSObjectBuffer = AddDisposable(new ConstantBuffer<LightingConstantsVS>(graphicsDevice, "GlobalLightingConstantsVS (objects)"));
            _globalLightingPSObjectBuffer = AddDisposable(new ConstantBuffer<LightingConstantsPS>(graphicsDevice, "GlobalLightingConstantsPS (objects)"));
            _shadowConstantsPSBuffer = AddDisposable(new ConstantBuffer<ShadowConstantsPS>(graphicsDevice, "ShadowConstantsPS"));

            _commandList = AddDisposable(graphicsDevice.ResourceFactory.CreateCommandList());

            _drawingContext = AddDisposable(new DrawingContext2D(
                game.ContentManager,
                BlendStateDescription.SingleAlphaBlend,
                GameOutputDescription));

            _shadowSampler = AddDisposable(graphicsDevice.ResourceFactory.CreateSampler(
                new SamplerDescription(
                    SamplerAddressMode.Clamp,
                    SamplerAddressMode.Clamp,
                    SamplerAddressMode.Clamp,
                    SamplerFilter.MinLinear_MagLinear_MipLinear,
                    ComparisonKind.LessEqual,
                    0,
                    0,
                    0,
                    0,
                    SamplerBorderColor.OpaqueBlack)));

            _shadowMapRenderer = AddDisposable(new ShadowMapRenderer());

            _intermediateSpriteBatch = AddDisposable(new SpriteBatch(game.ContentManager, BlendStateDescription.SingleDisabled, game.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription));
        }

        private void EnsureIntermediateFramebuffer(GraphicsDevice graphicsDevice, Framebuffer target)
        {
            if (_intermediateDepthBuffer != null && _intermediateDepthBuffer.Width == target.Width && _intermediateDepthBuffer.Height == target.Height)
            {
                return;
            }

            RemoveAndDispose(ref _intermediateDepthBuffer);
            RemoveAndDispose(ref _intermediateTexture);
            RemoveAndDispose(ref _intermediateFramebuffer);

            _intermediateDepthBuffer = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(target.Width, target.Height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil)));

            _intermediateTexture = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(target.Width, target.Height, 1, 1, target.ColorTargets[0].Target.Format, TextureUsage.RenderTarget | TextureUsage.Sampled)));

            _intermediateFramebuffer = AddDisposable(graphicsDevice.ResourceFactory.CreateFramebuffer(
                new FramebufferDescription(_intermediateDepthBuffer, _intermediateTexture)));
        }

        public void Execute(RenderContext context)
        {
            EnsureIntermediateFramebuffer(context.GraphicsDevice, context.RenderTarget);

            _renderList.Clear();

            context.Scene?.BuildRenderList(_renderList, context.Camera);

            foreach (var system in context.Game.GameSystems)
            {
                system.BuildRenderList(_renderList);
            }

            context.Game.RaiseBuildingRenderList(new BuildingRenderListEventArgs(
                _renderList,
                context.Camera));

            _commandList.Begin();

            if (context.Scene != null)
            {
                Render3DScene(_commandList, context.Scene, context);
            }

            // GUI and camera-dependent 2D elements
            {
                _drawingContext.Begin(
                    _commandList,
                    context.Game.ContentManager.LinearClampSampler,
                    new SizeF(context.Game.Viewport.Width, context.Game.Viewport.Height));

                context.Game.Scene3D?.Render(_drawingContext);
                context.Game.Scene2D.Render(_drawingContext);

                context.Game.RaiseRendering2D(new Rendering2DEventArgs(_drawingContext));

                _drawingContext.End();
            }

            _commandList.SetFramebuffer(context.RenderTarget);

            _intermediateSpriteBatch.Begin(
                _commandList,
                context.GraphicsDevice.PointSampler,
                new SizeF(_intermediateTexture.Width, _intermediateTexture.Height),
                ignoreAlpha: true);

            _intermediateSpriteBatch.DrawImage(_intermediateTexture, null, new Mathematics.RectangleF(0, 0, (int) _intermediateTexture.Width, (int) _intermediateTexture.Height), ColorRgbaF.White);

            _intermediateSpriteBatch.End();

            //_commandList.SetFramebuffer(context.RenderTarget);

            //_commandList.CopyTexture(_intermediateTexture, context.RenderTarget.ColorTargets[0].Target);

            _commandList.End();

            context.GraphicsDevice.SubmitCommands(_commandList);

            context.GraphicsDevice.SwapBuffers();
        }

        private void Render3DScene(CommandList commandList, Scene3D scene, RenderContext context)
        {
            Texture cloudTexture;
            if (scene.Lighting.TimeOfDay != TimeOfDay.Night
                && scene.Lighting.EnableCloudShadows
                && scene.Terrain != null)
            {
                cloudTexture = scene.Terrain.CloudTexture;
            }
            else
            {
                cloudTexture = context.Game.ContentManager.SolidWhiteTexture;
            }

            // Shadow map passes.

            _shadowMapRenderer.RenderShadowMap(
                scene,
                context.GraphicsDevice,
                ref _shadowConstants,
                (framebuffer, lightBoundingFrustum) =>
                {
                    commandList.SetFramebuffer(framebuffer);

                    commandList.ClearDepthStencil(1);

                    commandList.SetFullViewports();

                    var shadowViewProjection = lightBoundingFrustum.Matrix;
                    UpdateGlobalConstantBuffers(commandList, shadowViewProjection);

                    DoRenderPass(commandList, _renderList.Shadow, lightBoundingFrustum, null, null);
                });

            // Standard pass.

            commandList.SetFramebuffer(_intermediateFramebuffer);

            UpdateGlobalConstantBuffers(commandList, scene.Camera.View * scene.Camera.Projection);
            UpdateStandardPassConstantBuffers(commandList, context);

            commandList.ClearColorTarget(0, ColorRgba.DimGray.ToColorRgbaF().ToRgbaFloat());
            commandList.ClearDepthStencil(1);

            commandList.SetFullViewports();

            var standardPassCameraFrustum = context.Camera.BoundingFrustum;

            var shadowMap = _shadowMapRenderer.ShadowMap;

            DoRenderPass(commandList, _renderList.Opaque, standardPassCameraFrustum, cloudTexture, shadowMap);
            DoRenderPass(commandList, _renderList.Transparent, standardPassCameraFrustum, cloudTexture, shadowMap);
        }

        private void DoRenderPass(
            CommandList commandList,
            RenderBucket bucket,
            BoundingFrustum cameraFrustum,
            Texture cloudTexture,
            Texture shadowMap)
        {
            Culler.Cull(bucket.RenderItems, bucket.CulledItems, cameraFrustum);

            if (bucket.CulledItems.Count == 0)
            {
                return;
            }

            bucket.CulledItems.Sort();

            RenderItem? lastRenderItem = null;
            Matrix4x4? lastWorld = null;

            foreach (var renderItem in bucket.CulledItems)
            {
                if (lastRenderItem == null || lastRenderItem.Value.Effect != renderItem.Effect)
                {
                    var effect = renderItem.Effect;

                    effect.Begin(commandList);

                    SetDefaultMaterialProperties(renderItem.Material, cloudTexture, shadowMap);
                }

                if (lastRenderItem == null || lastRenderItem.Value.Material != renderItem.Material)
                {
                    renderItem.Material.ApplyPipelineState();
                    renderItem.Effect.ApplyPipelineState(commandList);
                }

                if (lastRenderItem == null || lastRenderItem.Value.VertexBuffer0 != renderItem.VertexBuffer0)
                {
                    commandList.SetVertexBuffer(0, renderItem.VertexBuffer0);
                }

                if (lastRenderItem == null || lastRenderItem.Value.VertexBuffer1 != renderItem.VertexBuffer1)
                {
                    if (renderItem.VertexBuffer1 != null)
                    {
                        commandList.SetVertexBuffer(1, renderItem.VertexBuffer1);
                    }
                }

                var renderItemConstantsVSParameter = renderItem.Effect.GetParameter("RenderItemConstantsVS", throwIfMissing: false);
                if (renderItemConstantsVSParameter != null)
                {
                    if (lastWorld == null || lastWorld.Value != renderItem.World)
                    {
                        _renderItemConstantsBufferVS.Value.World = renderItem.World;
                        _renderItemConstantsBufferVS.Update(commandList);

                        lastWorld = renderItem.World;
                    }

                    renderItem.Material.SetProperty(
                        "RenderItemConstantsVS",
                        _renderItemConstantsBufferVS.Buffer);
                }

                renderItem.Material.ApplyProperties();
                renderItem.Effect.ApplyParameters(commandList);

                switch (renderItem.DrawCommand)
                {
                    case DrawCommand.Draw:
                        commandList.Draw(
                            renderItem.VertexCount,
                            1,
                            renderItem.VertexStart,
                            0);
                        break;

                    case DrawCommand.DrawIndexed:
                        commandList.SetIndexBuffer(renderItem.IndexBuffer, IndexFormat.UInt16);
                        commandList.DrawIndexed(
                            renderItem.IndexCount,
                            1,
                            renderItem.StartIndex,
                            0,
                            0);
                        break;

                    default:
                        throw new System.Exception();
                }

                lastRenderItem = renderItem;
            }
        }

        private void SetDefaultMaterialProperties(
            EffectMaterial material,
            Texture cloudTexture,
            Texture shadowMap)
        {
            void setDefaultConstantBuffer(string name, DeviceBuffer buffer)
            {
                var parameter = material.Effect.GetParameter(name, throwIfMissing: false);
                if (parameter != null)
                {
                    material.SetProperty(name, buffer);
                }
            }

            setDefaultConstantBuffer("GlobalConstantsShared", _globalConstantBufferShared.Buffer);
            setDefaultConstantBuffer("GlobalConstantsVS", _globalConstantBufferVS.Buffer);
            setDefaultConstantBuffer("GlobalConstantsPS", _globalConstantBufferPS.Buffer);

            switch (material.LightingType)
            {
                case LightingType.Terrain:
                    setDefaultConstantBuffer("GlobalLightingConstantsVS", _globalLightingVSTerrainBuffer.Buffer);
                    setDefaultConstantBuffer("GlobalLightingConstantsPS", _globalLightingPSTerrainBuffer.Buffer);
                    break;

                case LightingType.Object:
                    setDefaultConstantBuffer("GlobalLightingConstantsVS", _globalLightingVSObjectBuffer.Buffer);
                    setDefaultConstantBuffer("GlobalLightingConstantsPS", _globalLightingPSObjectBuffer.Buffer);
                    break;
            }

            var cloudTextureParameter = material.Effect.GetParameter("Global_CloudTexture", throwIfMissing: false);
            if (cloudTextureParameter != null)
            {
                material.SetProperty("Global_CloudTexture", cloudTexture);
            }

            setDefaultConstantBuffer("ShadowConstantsPS", _shadowConstantsPSBuffer.Buffer);

            var shadowMapParameter = material.Effect.GetParameter("Global_ShadowMap", throwIfMissing: false);
            if (shadowMapParameter != null)
            {
                material.SetProperty("Global_ShadowMap", shadowMap);
                material.SetProperty("Global_ShadowSampler", _shadowSampler);
            }
        }

        private void UpdateGlobalConstantBuffers(
            CommandList commandEncoder,
            in Matrix4x4 viewProjection)
        {
            _globalConstantBufferVS.Value.ViewProjection = viewProjection;
            _globalConstantBufferVS.Update(commandEncoder);
        }

        private void UpdateStandardPassConstantBuffers(
            CommandList commandEncoder,
            RenderContext context)
        {
            var cloudShadowView = Matrix4x4.CreateLookAt(
                Vector3.Zero,
                Vector3.Normalize(new Vector3(0, 0.2f, -1)),
                Vector3.UnitY);

            var cloudShadowProjection = Matrix4x4.CreateOrthographic(1, 1, 0, 1);

            var lightingConstantsVS = new LightingConstantsVS
            {
                CloudShadowMatrix = cloudShadowView * cloudShadowProjection
            };

            var cameraPosition = Matrix4x4Utility.Invert(context.Camera.View).Translation;

            _globalConstantBufferShared.Value.CameraPosition = cameraPosition;
            _globalConstantBufferShared.Value.TimeInSeconds = (float) context.GameTime.TotalGameTime.TotalSeconds;
            _globalConstantBufferShared.Update(commandEncoder);

            void updateLightingBuffer(
                ConstantBuffer<LightingConstantsVS> bufferVS,
                ConstantBuffer<LightingConstantsPS> bufferPS,
                in LightingConstantsPS constantsPS)
            {
                bufferVS.Value = lightingConstantsVS;
                bufferVS.Update(commandEncoder);

                bufferPS.Value = constantsPS;
                bufferPS.Update(commandEncoder);
            }

            updateLightingBuffer(
                _globalLightingVSTerrainBuffer,
                _globalLightingPSTerrainBuffer,
                context.Scene.Lighting.CurrentLightingConfiguration.TerrainLightsPS);

            updateLightingBuffer(
                _globalLightingVSObjectBuffer,
                _globalLightingPSObjectBuffer,
                context.Scene.Lighting.CurrentLightingConfiguration.ObjectLightsPS);

            _globalConstantBufferPS.Value.ViewportSize = new Vector2(context.Game.Viewport.Width, context.Game.Viewport.Height);
            _globalConstantBufferPS.Update(commandEncoder);

            _shadowConstantsPSBuffer.Value = _shadowConstants;
            _shadowConstantsPSBuffer.Update(commandEncoder);
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct GlobalConstantsShared
        {
            public Vector3 CameraPosition;
            public float TimeInSeconds;
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

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct GlobalConstantsPS
        {
            public Vector2 ViewportSize;
        }
    }
}
