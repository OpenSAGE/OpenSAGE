using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.Map;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderPipeline : DisposableBase
    {
        public static readonly OutputDescription GameOutputDescription = new OutputDescription(
            new OutputAttachmentDescription(PixelFormat.D32_Float_S8_UInt),
            new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));

        public const PixelFormat ShadowMapPixelFormat = PixelFormat.D32_Float_S8_UInt;

        public static readonly OutputDescription DepthPassDescription = new OutputDescription(
            new OutputAttachmentDescription(ShadowMapPixelFormat));

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

        private readonly DrawingContext2D _drawingContext;

        private readonly ShadowMapRenderer _shadowMapRenderer;
        private readonly ConstantBuffer<ShadowHelpers.Global_ShadowConstantsPS> _globalShadowConstantsBufferPS;

        private readonly Sampler _shadowSampler;

        public RenderPipeline(Game game)
        {
            _renderList = new RenderList();

            var graphicsDevice = game.GraphicsDevice;

            _globalConstantBufferShared = AddDisposable(new ConstantBuffer<GlobalConstantsShared>(graphicsDevice, "GlobalConstantsShared"));
            _globalConstantBufferVS = AddDisposable(new ConstantBuffer<GlobalConstantsVS>(graphicsDevice, "GlobalConstantsVS"));
            _renderItemConstantsBufferVS = AddDisposable(new ConstantBuffer<RenderItemConstantsVS>(graphicsDevice, "RenderItemConstantsVS"));
            _globalConstantBufferPS = AddDisposable(new ConstantBuffer<GlobalConstantsPS>(graphicsDevice, "GlobalConstantsPS"));
            _globalLightingVSTerrainBuffer = AddDisposable(new ConstantBuffer<LightingConstantsVS>(graphicsDevice, "Global_LightingConstantsVS (terrain)"));
            _globalLightingPSTerrainBuffer = AddDisposable(new ConstantBuffer<LightingConstantsPS>(graphicsDevice, "Global_LightingConstantsPS (terrain)"));
            _globalLightingVSObjectBuffer = AddDisposable(new ConstantBuffer<LightingConstantsVS>(graphicsDevice, "Global_LightingConstantsVS (objects)"));
            _globalLightingPSObjectBuffer = AddDisposable(new ConstantBuffer<LightingConstantsPS>(graphicsDevice, "Global_LightingConstantsPS (objects)"));

            _commandList = AddDisposable(graphicsDevice.ResourceFactory.CreateCommandList());

            _drawingContext = AddDisposable(new DrawingContext2D(
                game.ContentManager,
                BlendStateDescription.SingleAlphaBlend,
                GameOutputDescription));

            _shadowMapRenderer = AddDisposable(new ShadowMapRenderer(graphicsDevice));

            _globalShadowConstantsBufferPS = AddDisposable(new ConstantBuffer<ShadowHelpers.Global_ShadowConstantsPS>(graphicsDevice, "GlobalShadowConstantsPS"));
            _globalShadowConstantsBufferPS.Value.Initialize();

            _shadowSampler = AddDisposable(graphicsDevice.ResourceFactory.CreateSampler(
                new SamplerDescription
                {
                    AddressModeU = SamplerAddressMode.Clamp,
                    AddressModeV = SamplerAddressMode.Clamp,
                    AddressModeW = SamplerAddressMode.Clamp,
                    Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
                    ComparisonKind = ComparisonKind.LessEqual,
                    MaximumLod = uint.MaxValue
                }));
        }

        public void Execute(RenderContext context)
        {
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

            _shadowMapRenderer.RenderShadowMaps(
                commandList,
                scene.Lighting,
                scene.Camera,
                ref _globalShadowConstantsBufferPS.Value,
                out var shadowMap,
                shadowCamera =>
                {
                    UpdateGlobalConstantBuffers(commandList, shadowCamera);
                    DoRenderPass(commandList, _renderList.Shadow, shadowCamera, null, null);
                });

            // Standard pass.

            UpdateGlobalConstantBuffers(commandList, scene.Camera);
            UpdateStandardPassConstantBuffers(commandList, context);

            commandList.SetFramebuffer(context.RenderTarget);

            commandList.ClearColorTarget(0, ColorRgba.DimGray.ToColorRgbaF().ToRgbaFloat());
            commandList.ClearDepthStencil(1);

            commandList.SetFullViewports();

            DoRenderPass(commandList, _renderList.Opaque, context.Camera, cloudTexture, shadowMap);
            DoRenderPass(commandList, _renderList.Transparent, context.Camera, cloudTexture, shadowMap);
        }

        private void DoRenderPass(
            CommandList commandList,
            RenderBucket bucket,
            Camera camera,
            Texture cloudTexture,
            Texture shadowMap)
        {
            Culler.Cull(bucket.RenderItems, bucket.CulledItems, camera.BoundingFrustum);

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

                var renderItemConstantsVSParameter = renderItem.Effect.GetParameter("RenderItemConstantsVS");
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

        private void SetDefaultMaterialProperties(EffectMaterial material, Texture cloudTexture, Texture shadowMap)
        {
            void SetDefaultResource(string name, BindableResource resource)
            {
                var parameter = material.Effect.GetParameter(name, throwIfMissing: false);
                if (parameter != null)
                {
                    material.SetProperty(name, resource);
                }
            }

            void SetDefaultTexture(string name, Texture texture)
            {
                var parameter = material.Effect.GetParameter(name, throwIfMissing: false);
                if (parameter != null)
                {
                    material.SetProperty(name, texture);
                }
            }

            SetDefaultResource("GlobalConstantsShared", _globalConstantBufferShared.Buffer);
            SetDefaultResource("GlobalConstantsVS", _globalConstantBufferVS.Buffer);
            SetDefaultResource("GlobalConstantsPS", _globalConstantBufferPS.Buffer);

            switch (material.LightingType)
            {
                case LightingType.Terrain:
                    SetDefaultResource("Global_LightingConstantsVS", _globalLightingVSTerrainBuffer.Buffer);
                    SetDefaultResource("Global_LightingConstantsPS", _globalLightingPSTerrainBuffer.Buffer);
                    break;

                case LightingType.Object:
                    SetDefaultResource("Global_LightingConstantsVS", _globalLightingVSObjectBuffer.Buffer);
                    SetDefaultResource("Global_LightingConstantsPS", _globalLightingPSObjectBuffer.Buffer);
                    break;
            }

            SetDefaultResource("GlobalShadowConstantsPS", _globalShadowConstantsBufferPS.Buffer);

            SetDefaultTexture("Global_CloudTexture", cloudTexture);

            SetDefaultTexture("Global_ShadowMap", shadowMap);
            SetDefaultResource("Global_ShadowSampler", _shadowSampler);
        }

        private void UpdateGlobalConstantBuffers(
            CommandList commandEncoder,
            Camera camera)
        {
            _globalConstantBufferVS.Value.ViewProjection = camera.ViewProjection;
            _globalConstantBufferVS.Update(commandEncoder);
        }

        private void UpdateStandardPassConstantBuffers(
            CommandList commandList,
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
            _globalConstantBufferShared.Update(commandList);

            void updateLightingBuffer(
                ConstantBuffer<LightingConstantsVS> bufferVS,
                ConstantBuffer<LightingConstantsPS> bufferPS,
                in LightingConstantsPS constantsPS)
            {
                bufferVS.Value = lightingConstantsVS;
                bufferVS.Update(commandList);

                bufferPS.Value = constantsPS;
                bufferPS.Update(commandList);
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
            _globalConstantBufferPS.Update(commandList);

            commandList.UpdateBuffer(_globalShadowConstantsBufferPS.Buffer, 0, _globalShadowConstantsBufferPS.Value.GetBlittable());
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
