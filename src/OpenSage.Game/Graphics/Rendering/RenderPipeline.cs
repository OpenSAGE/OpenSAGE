using System;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Graphics.Rendering.Shadows;
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

        public static readonly OutputDescription GameOutputDescription = new OutputDescription(
            new OutputAttachmentDescription(PixelFormat.D24_UNorm_S8_UInt),
            new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));

        private readonly RenderList _renderList;

        private readonly CommandList _commandList;

        private readonly GlobalResources _globalResources;

        private readonly ConstantBuffer<MeshTypes.RenderItemConstantsVS> _renderItemConstantsBufferVS;
        private readonly ConstantBuffer<MeshTypes.RenderItemConstantsPS> _renderItemConstantsBufferPS;
        private readonly ResourceSet _renderItemConstantsResourceSet;

        private readonly DrawingContext2D _drawingContext;

        private readonly ShadowMapRenderer _shadowMapRenderer;

        private Texture _intermediateDepthBuffer;
        private Texture _intermediateTexture;
        private Framebuffer _intermediateFramebuffer;

        private readonly TextureCopier _textureCopier;

        public Texture ShadowMap => _shadowMapRenderer.ShadowMap;

        public int RenderedObjectsOpaque { get; private set; }
        public int RenderedObjectsTransparent { get; private set; }

        public RenderPipeline(Game game)
        {
            _renderList = new RenderList();

            var graphicsDevice = game.GraphicsDevice;

            _globalResources = AddDisposable(new GlobalResources(graphicsDevice, game.ContentManager));

            _renderItemConstantsBufferVS = AddDisposable(new ConstantBuffer<MeshTypes.RenderItemConstantsVS>(graphicsDevice, "RenderItemConstantsVS"));

            _renderItemConstantsBufferPS = AddDisposable(new ConstantBuffer<MeshTypes.RenderItemConstantsPS>(graphicsDevice, "RenderItemConstantsPS"));

            var renderItemConstantsResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("RenderItemConstantsVS", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("RenderItemConstantsPS", ResourceKind.UniformBuffer, ShaderStages.Fragment))));

            _renderItemConstantsResourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    renderItemConstantsResourceLayout,
                    _renderItemConstantsBufferVS.Buffer,
                    _renderItemConstantsBufferPS.Buffer)));

            _commandList = AddDisposable(graphicsDevice.ResourceFactory.CreateCommandList());

            _drawingContext = AddDisposable(new DrawingContext2D(
                game.ContentManager,
                BlendStateDescription.SingleAlphaBlend,
                GameOutputDescription));

            _shadowMapRenderer = AddDisposable(new ShadowMapRenderer(game.GraphicsDevice));

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
                _commandList.ClearColorTarget(0, ColorRgba.DimGray.ToColorRgbaF().ToRgbaFloat());
            }

            // GUI and camera-dependent 2D elements
            {
                _commandList.PushDebugGroup("2D Scene");

                _drawingContext.Begin(
                    _commandList,
                    context.ContentManager.LinearClampSampler,
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

        private void Render3DScene(CommandList commandList, Scene3D scene, RenderContext context)
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
                cloudResourceSet = _globalResources.DefaultCloudResourceSet;
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
                    _globalResources.UpdateGlobalConstantBuffers(commandList, shadowViewProjection);

                    DoRenderPass(commandList, _renderList.Shadow, lightBoundingFrustum, null);
                });

            commandList.PopDebugGroup();

            // Standard pass.

            commandList.PushDebugGroup("Forward pass");

            commandList.SetFramebuffer(_intermediateFramebuffer);

            _globalResources.UpdateGlobalConstantBuffers(commandList, scene.Camera.ViewProjection);
            _globalResources.UpdateStandardPassConstantBuffers(commandList, context);

            commandList.ClearColorTarget(0, ColorRgba.DimGray.ToColorRgbaF().ToRgbaFloat());
            commandList.ClearDepthStencil(1);

            commandList.SetFullViewports();

            var standardPassCameraFrustum = scene.Camera.BoundingFrustum;

            commandList.PushDebugGroup("Opaque");
            RenderedObjectsOpaque = DoRenderPass(commandList, _renderList.Opaque, standardPassCameraFrustum, cloudResourceSet);
            commandList.PopDebugGroup();

            commandList.PushDebugGroup("Transparent");
            RenderedObjectsTransparent = DoRenderPass(commandList, _renderList.Transparent, standardPassCameraFrustum, cloudResourceSet);
            commandList.PopDebugGroup();

            commandList.PopDebugGroup();
        }

        private int DoRenderPass(
            CommandList commandList,
            RenderBucket bucket,
            BoundingFrustum cameraFrustum,
            ResourceSet cloudResourceSet)
        {
            // TODO: Make culling batch size configurable at runtime
            bucket.RenderItems.CullAndSort(cameraFrustum, ParallelCullingBatchSize);

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
                }

                if (lastRenderItemIndex == null || bucket.RenderItems[lastRenderItemIndex.Value].ShaderSet != renderItem.ShaderSet)
                {
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

                    if (renderItem.HouseColor != null)
                    {
                        var houseColor = renderItem.HouseColor.Value.ToVector3();
                        if (houseColor != _renderItemConstantsBufferPS.Value.HouseColor)
                        {
                            _renderItemConstantsBufferPS.Value.HouseColor = houseColor;
                            _renderItemConstantsBufferPS.Update(commandList);
                        }
                    }
                }

                renderItem.BeforeRenderCallback.Invoke(commandList);

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
                commandList.SetGraphicsResourceSet(indices.GlobalConstants.Value, _globalResources.GlobalConstantsResourceSet);
            }

            switch (indices.LightingType)
            {
                case LightingType.Terrain:
                    commandList.SetGraphicsResourceSet(indices.GlobalLightingConstants.Value, _globalResources.GlobalLightingConstantsTerrainResourceSet);
                    break;

                case LightingType.Object:
                    commandList.SetGraphicsResourceSet(indices.GlobalLightingConstants.Value, _globalResources.GlobalLightingConstantsObjectResourceSet);
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
