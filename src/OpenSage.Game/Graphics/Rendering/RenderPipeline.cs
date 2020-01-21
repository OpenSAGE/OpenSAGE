using System;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Map;
using OpenSage.Graphics.Rendering.Shadows;
using OpenSage.Graphics.Rendering.Water;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderPipeline : DisposableBase
    {
        public event EventHandler<Rendering2DEventArgs> Rendering2D;
        public event EventHandler<BuildingRenderListEventArgs> BuildingRenderList;

        private const int ParallelCullingBatchSize = 128;

        private static readonly RgbaFloat ClearColor = new RgbaFloat(105, 105, 105, 255);

        public static readonly OutputDescription GameOutputDescription = new OutputDescription(
            new OutputAttachmentDescription(PixelFormat.D24_UNorm_S8_UInt),
            new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));

        private readonly RenderList _renderList;

        private readonly CommandList _commandList;

        private readonly GraphicsLoadContext _loadContext;
        private readonly GlobalShaderResources _globalShaderResources;
        private readonly GlobalShaderResourceData _globalShaderResourceData;

        private readonly ConstantBuffer<MeshShaderResources.RenderItemConstantsVS> _renderItemConstantsBufferVS;
        private readonly ConstantBuffer<MeshShaderResources.RenderItemConstantsPS> _renderItemConstantsBufferPS;
        private readonly ResourceSet _renderItemConstantsResourceSet;

        private readonly DrawingContext2D _drawingContext;

        private readonly ShadowMapRenderer _shadowMapRenderer;
        private readonly WaterMapRenderer _waterMapRenderer;

        private Texture _intermediateDepthBuffer;
        private Texture _intermediateTexture;
        private Framebuffer _intermediateFramebuffer;

        private readonly TextureCopier _textureCopier;

        public Texture ShadowMap => _shadowMapRenderer.ShadowMap;
        public Texture ReflectionMap => _waterMapRenderer.ReflectionMap;
        public Texture RefractionMap => _waterMapRenderer.RefractionMap;

        public int RenderedObjectsOpaque { get; private set; }
        public int RenderedObjectsTransparent { get; private set; }

        public RenderPipeline(Game game)
        {
            _renderList = new RenderList();

            var graphicsDevice = game.GraphicsDevice;

            _loadContext = game.GraphicsLoadContext;

            _globalShaderResources = game.GraphicsLoadContext.ShaderResources.Global;
            _globalShaderResourceData = AddDisposable(new GlobalShaderResourceData(game.GraphicsDevice, _globalShaderResources));

            _renderItemConstantsBufferVS = AddDisposable(new ConstantBuffer<MeshShaderResources.RenderItemConstantsVS>(graphicsDevice, "RenderItemConstantsVS"));
            _renderItemConstantsBufferPS = AddDisposable(new ConstantBuffer<MeshShaderResources.RenderItemConstantsPS>(graphicsDevice, "RenderItemConstantsPS"));

            _renderItemConstantsResourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    game.GraphicsLoadContext.ShaderResources.Mesh.RenderItemConstantsResourceLayout,
                    _renderItemConstantsBufferVS.Buffer,
                    _renderItemConstantsBufferPS.Buffer)));

            _commandList = AddDisposable(graphicsDevice.ResourceFactory.CreateCommandList());

            _drawingContext = AddDisposable(new DrawingContext2D(
                game.ContentManager,
                game.GraphicsLoadContext,
                BlendStateDescription.SingleAlphaBlend,
                GameOutputDescription));

            _shadowMapRenderer = AddDisposable(new ShadowMapRenderer(game.GraphicsDevice, game.GraphicsLoadContext.ShaderResources.Global));
            _waterMapRenderer = AddDisposable(new WaterMapRenderer(game.AssetStore, _loadContext, game.GraphicsDevice, game.GraphicsLoadContext.ShaderResources.Global));

            _textureCopier = AddDisposable(new TextureCopier(
                game,
                game.Panel.OutputDescription));
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
            RenderedObjectsOpaque = 0;
            RenderedObjectsTransparent = 0;

            EnsureIntermediateFramebuffer(context.GraphicsDevice, context.RenderTarget);

            _renderList.Clear();

            context.Scene3D?.BuildRenderList(
                _renderList,
                context.Scene3D.Camera,
                context.GameTime);

            BuildingRenderList?.Invoke(this, new BuildingRenderListEventArgs(
                _renderList,
                context.Scene3D?.Camera,
                context.GameTime));

            _commandList.Begin();

            if (context.Scene3D != null)
            {
                _commandList.PushDebugGroup("3D Scene");
                Render3DScene(_commandList, context.Scene3D, context);
                _commandList.PopDebugGroup();
            }
            else
            {
                _commandList.SetFramebuffer(_intermediateFramebuffer);
                _commandList.ClearColorTarget(0, ClearColor);
            }

            // GUI and camera-dependent 2D elements
            {
                _commandList.PushDebugGroup("2D Scene");

                _drawingContext.Begin(
                    _commandList,
                    _loadContext.StandardGraphicsResources.LinearClampSampler,
                    new SizeF(context.RenderTarget.Width, context.RenderTarget.Height));

                context.Scene3D?.Render(_drawingContext);
                context.Scene2D?.Render(_drawingContext);

                Rendering2D?.Invoke(this, new Rendering2DEventArgs(_drawingContext));

                _drawingContext.End();

                _commandList.PopDebugGroup();
            }

            _commandList.End();

            context.GraphicsDevice.SubmitCommands(_commandList);

            _textureCopier.Execute(
                _intermediateTexture,
                context.RenderTarget);
        }

        private void Render3DScene(
            CommandList commandList,
            Scene3D scene,
            RenderContext context)
        {
            ResourceSet cloudResourceSet;
            if (scene.Lighting.TimeOfDay != TimeOfDay.Night
                && scene.Lighting.EnableCloudShadows
                && scene.Terrain != null)
            {
                cloudResourceSet = scene.Terrain.CloudResourceSet;
            }
            else
            {
                cloudResourceSet = _globalShaderResources.DefaultCloudResourceSet;
            }

            // Shadow map passes.

            commandList.PushDebugGroup("Shadow pass");

            _shadowMapRenderer.RenderShadowMap(
                scene,
                context.GraphicsDevice,
                commandList,
                (framebuffer, lightBoundingFrustum) =>
                {
                    commandList.SetFramebuffer(framebuffer);

                    commandList.ClearDepthStencil(1);

                    commandList.SetFullViewports();

                    var shadowViewProjection = lightBoundingFrustum.Matrix;
                    _globalShaderResourceData.UpdateGlobalConstantBuffers(commandList, scene.Camera.Projection, shadowViewProjection, new Vector4(0, 0, 0, 0));

                    DoRenderPass(context, commandList, _renderList.Shadow, lightBoundingFrustum, null);
                });

            commandList.PopDebugGroup();

            // Standard pass.

            commandList.PushDebugGroup("Forward pass");

            commandList.SetFramebuffer(_intermediateFramebuffer);

            _globalShaderResourceData.UpdateGlobalConstantBuffers(commandList, scene.Camera.Projection, scene.Camera.ViewProjection, new Vector4(0, 0, 0, 0));
            _globalShaderResourceData.UpdateStandardPassConstantBuffers(commandList, context);

            commandList.ClearColorTarget(0, ClearColor);
            commandList.ClearDepthStencil(1);

            commandList.SetFullViewports();

            var standardPassCameraFrustum = scene.Camera.BoundingFrustum;

            commandList.PushDebugGroup("Terrain");
            RenderedObjectsOpaque += DoRenderPass(context, commandList, _renderList.Terrain, standardPassCameraFrustum, cloudResourceSet);
            commandList.PopDebugGroup();

            commandList.PushDebugGroup("Road");
            RenderedObjectsOpaque += DoRenderPass(context, commandList, _renderList.Road, standardPassCameraFrustum, cloudResourceSet);
            commandList.PopDebugGroup();

            commandList.PushDebugGroup("Water");
            DoRenderPass(context, commandList, _renderList.Water, standardPassCameraFrustum, cloudResourceSet);
            commandList.PopDebugGroup();

            commandList.PushDebugGroup("Opaque");
            RenderedObjectsOpaque += DoRenderPass(context, commandList, _renderList.Opaque, standardPassCameraFrustum, cloudResourceSet);
            commandList.PopDebugGroup();

            commandList.PushDebugGroup("Transparent");
            RenderedObjectsTransparent = DoRenderPass(context, commandList, _renderList.Transparent, standardPassCameraFrustum, cloudResourceSet);
            commandList.PopDebugGroup();

            commandList.PopDebugGroup();
        }

        private void CalculateWaterShaderMap(Scene3D scene, RenderContext context, CommandList commandList, RenderItem renderItem, ResourceSet cloudResourceSet)
        {
            _waterMapRenderer.RenderWaterShaders(
                scene,
                context.GraphicsDevice,
                commandList,
                (reflectionFramebuffer, refractionFramebuffer) =>
                {
                    var camera = scene.Camera;
                    var clippingOffset = scene.Waters.ClippingOffset;
                    var originalFarPlaneDistance = camera.FarPlaneDistance;
                    var pivot = renderItem.World.Translation.Y;

                    if (refractionFramebuffer != null)
                    {
                        commandList.PushDebugGroup("Refraction");
                        camera.FarPlaneDistance = scene.Waters.RefractionRenderDistance;
                        var clippingPlane = new ClippingPlane(new Vector4(0, 0, -1, pivot + clippingOffset));

                        // Render normal scene for water refraction shader
                        _globalShaderResourceData.UpdateGlobalConstantBuffers(commandList, camera.Projection, camera.ViewProjection, clippingPlane.ConvertToVector4());
                        _globalShaderResourceData.UpdateStandardPassConstantBuffers(commandList, context);

                        commandList.SetFramebuffer(refractionFramebuffer);

                        commandList.ClearColorTarget(0, ClearColor);
                        commandList.ClearDepthStencil(1);

                        commandList.SetFullViewports();

                        RenderedObjectsOpaque += DoRenderPass(context, commandList, _renderList.Terrain, camera.BoundingFrustum, cloudResourceSet, clippingPlane);
                        RenderedObjectsOpaque += DoRenderPass(context, commandList, _renderList.Opaque, camera.BoundingFrustum, cloudResourceSet, clippingPlane);
                        commandList.PopDebugGroup();
                    }

                    if (reflectionFramebuffer != null)
                    {
                        commandList.PushDebugGroup("Reflection");
                        camera.FarPlaneDistance = scene.Waters.ReflectionRenderDistance;
                        var clippingPlane = new ClippingPlane(new Vector4(0, 0, 1, -pivot - clippingOffset));

                        // TODO: Improve rendering speed somehow?
                        // ------------------- Used for creating stencil mask -------------------
                        _globalShaderResourceData.UpdateGlobalConstantBuffers(commandList, camera.Projection, camera.ViewProjection, clippingPlane.ConvertToVector4());

                        commandList.SetFramebuffer(reflectionFramebuffer);
                        commandList.ClearColorTarget(0, ClearColor);
                        commandList.ClearDepthStencil(1);

                        RenderedObjectsOpaque += DoRenderPass(context, commandList, _renderList.Terrain, camera.BoundingFrustum, cloudResourceSet, clippingPlane);
                        // -----------------------------------------------------------------------

                        // Render inverted scene for water reflection shader
                        camera.SetMirrorX(pivot);
                        _globalShaderResourceData.UpdateGlobalConstantBuffers(commandList, camera.Projection, camera.ViewProjection, clippingPlane.ConvertToVector4());

                        //commandList.SetFramebuffer(reflectionFramebuffer);
                        commandList.ClearColorTarget(0, ClearColor);
                        //commandList.ClearDepthStencil(1);

                        commandList.SetFullViewports();

                        RenderedObjectsOpaque += DoRenderPass(context, commandList, _renderList.Terrain, camera.BoundingFrustum, cloudResourceSet, clippingPlane);
                        RenderedObjectsOpaque += DoRenderPass(context, commandList, _renderList.Opaque, camera.BoundingFrustum, cloudResourceSet, clippingPlane);

                        camera.SetMirrorX(pivot);
                        commandList.PopDebugGroup();
                    }

                    if (reflectionFramebuffer != null || refractionFramebuffer != null)
                    {
                        camera.FarPlaneDistance = originalFarPlaneDistance;
                        _globalShaderResourceData.UpdateGlobalConstantBuffers(commandList, camera.Projection, camera.ViewProjection, new Vector4(0, 0, 0, 0));
                        _globalShaderResourceData.UpdateStandardPassConstantBuffers(commandList, context);

                        // Reset the render item pipeline
                        commandList.SetFramebuffer(_intermediateFramebuffer);
                        commandList.InsertDebugMarker("Setting pipeline");
                        commandList.SetPipeline(renderItem.Pipeline);
                    }

                    SetGlobalResources(commandList, renderItem.ShaderSet.GlobalResourceSetIndices, cloudResourceSet);
                    commandList.SetGraphicsResourceSet(4, _waterMapRenderer.ResourceSetForRendering);
                });
        }

        private int DoRenderPass(
            RenderContext context,
            CommandList commandList,
            RenderBucket bucket,
            BoundingFrustum cameraFrustum,
            ResourceSet cloudResourceSet,
            ClippingPlane clippingPlane = null)
        {
            // TODO: Make culling batch size configurable at runtime
            bucket.RenderItems.CullAndSort(cameraFrustum, clippingPlane, ParallelCullingBatchSize);

            if (bucket.RenderItems.CulledItemIndices.Count == 0)
            {
                return 0;
            }

            Matrix4x4? lastWorld = null;
            int? lastRenderItemIndex = null;

            foreach (var i in bucket.RenderItems.CulledItemIndices)
            {
                ref var renderItem = ref bucket.RenderItems[i];

                if (lastRenderItemIndex == null || bucket.RenderItems[lastRenderItemIndex.Value].Pipeline != renderItem.Pipeline)
                {
                    commandList.InsertDebugMarker("Setting pipeline");
                    commandList.SetPipeline(renderItem.Pipeline);
                    SetGlobalResources(commandList, renderItem.ShaderSet.GlobalResourceSetIndices, cloudResourceSet);
                }

                if (renderItem.ShaderSet.GlobalResourceSetIndices.RenderItemConstants != null)
                {
                    if (lastWorld == null || lastWorld.Value != renderItem.World)
                    {
                        _renderItemConstantsBufferVS.Value.World = renderItem.World;
                        _renderItemConstantsBufferVS.Update(commandList);

                        lastWorld = renderItem.World;
                    }

                    if (renderItem.RenderItemConstantsPS != null)
                    {
                        _renderItemConstantsBufferPS.Value = renderItem.RenderItemConstantsPS.Value;
                        _renderItemConstantsBufferPS.Update(commandList);
                    }
                }

                if (bucket.RenderItemName == "Water")
                {
                    CalculateWaterShaderMap(context.Scene3D, context, commandList, renderItem, cloudResourceSet);
                }
                renderItem.BeforeRenderCallback.Invoke(commandList, context);

                commandList.SetIndexBuffer(renderItem.IndexBuffer, IndexFormat.UInt16);
                commandList.DrawIndexed(
                    renderItem.IndexCount,
                    1,
                    renderItem.StartIndex,
                    0,
                    0);

                lastRenderItemIndex = i;
            }

            return bucket.RenderItems.CulledItemIndices.Count;
        }

        private void SetGlobalResources(
            CommandList commandList,
            GlobalResourceSetIndices indices,
            ResourceSet cloudResourceSet)
        {
            if (indices.GlobalConstants != null)
            {
                commandList.SetGraphicsResourceSet(indices.GlobalConstants.Value, _globalShaderResourceData.GlobalConstantsResourceSet);
            }

            switch (indices.LightingType)
            {
                case LightingType.Terrain:
                    commandList.SetGraphicsResourceSet(indices.GlobalLightingConstants.Value, _globalShaderResourceData.GlobalLightingConstantsTerrainResourceSet);
                    break;

                case LightingType.Object:
                    commandList.SetGraphicsResourceSet(indices.GlobalLightingConstants.Value, _globalShaderResourceData.GlobalLightingConstantsObjectResourceSet);
                    break;
            }

            if (indices.CloudConstants != null)
            {
                commandList.SetGraphicsResourceSet(indices.CloudConstants.Value, cloudResourceSet);
            }

            if (indices.ShadowConstants != null)
            {
                commandList.SetGraphicsResourceSet(indices.ShadowConstants.Value, _shadowMapRenderer.ResourceSetForRendering);
            }

            if (indices.RenderItemConstants != null)
            {
                commandList.SetGraphicsResourceSet(indices.RenderItemConstants.Value, _renderItemConstantsResourceSet);
            }
        }
    }
}
