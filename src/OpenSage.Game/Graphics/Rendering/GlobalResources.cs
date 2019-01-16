using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Map;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class GlobalResources : DisposableBase
    {
        private readonly ConstantBuffer<GlobalTypes.GlobalConstantsShared> _globalConstantBufferShared;
        private readonly ConstantBuffer<GlobalTypes.GlobalConstantsVS> _globalConstantBufferVS;
        private readonly ConstantBuffer<GlobalTypes.GlobalConstantsPS> _globalConstantBufferPS;

        private readonly ConstantBuffer<GlobalLightingTypes.LightingConstantsVS> _globalLightingBufferVS;
        private readonly ConstantBuffer<GlobalLightingTypes.LightingConstantsPS> _globalLightingTerrainBufferPS;
        private readonly ConstantBuffer<GlobalLightingTypes.LightingConstantsPS> _globalLightingObjectBufferPS;

        public readonly ResourceSet GlobalConstantsResourceSet;
        public readonly ResourceSet GlobalLightingConstantsTerrainResourceSet;
        public readonly ResourceSet GlobalLightingConstantsObjectResourceSet;
        public readonly ResourceSet DefaultCloudResourceSet;

        private TimeOfDay? _cachedTimeOfDay;

        public GlobalResources(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            _globalConstantBufferShared = AddDisposable(new ConstantBuffer<GlobalTypes.GlobalConstantsShared>(graphicsDevice, "GlobalConstantsShared"));
            _globalConstantBufferVS = AddDisposable(new ConstantBuffer<GlobalTypes.GlobalConstantsVS>(graphicsDevice, "GlobalConstantsVS"));
            _globalConstantBufferPS = AddDisposable(new ConstantBuffer<GlobalTypes.GlobalConstantsPS>(graphicsDevice, "GlobalConstantsPS"));

            var globalConstantsResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("GlobalConstantsShared", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("GlobalConstantsVS", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("GlobalConstantsPS", ResourceKind.UniformBuffer, ShaderStages.Fragment))));

            GlobalConstantsResourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    globalConstantsResourceLayout,
                    _globalConstantBufferShared.Buffer,
                    _globalConstantBufferVS.Buffer,
                    _globalConstantBufferPS.Buffer)));

            _globalLightingBufferVS = AddDisposable(new ConstantBuffer<GlobalLightingTypes.LightingConstantsVS>(graphicsDevice, "GlobalLightingConstantsVS (terrain)"));
            SetGlobalLightingBufferVS(graphicsDevice);

            _globalLightingTerrainBufferPS = AddDisposable(new ConstantBuffer<GlobalLightingTypes.LightingConstantsPS>(graphicsDevice, "GlobalLightingConstantsPS (terrain)"));
            _globalLightingObjectBufferPS = AddDisposable(new ConstantBuffer<GlobalLightingTypes.LightingConstantsPS>(graphicsDevice, "GlobalLightingConstantsPS (objects)"));

            var globalLightingConstantsResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("GlobalLightingConstantsVS", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("GlobalLightingConstantsPS", ResourceKind.UniformBuffer, ShaderStages.Fragment))));

            GlobalLightingConstantsTerrainResourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    globalLightingConstantsResourceLayout,
                    _globalLightingBufferVS.Buffer,
                    _globalLightingTerrainBufferPS.Buffer)));

            GlobalLightingConstantsObjectResourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    globalLightingConstantsResourceLayout,
                    _globalLightingBufferVS.Buffer,
                    _globalLightingObjectBufferPS.Buffer)));

            var cloudResoureLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Global_CloudTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment))));

            DefaultCloudResourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    cloudResoureLayout,
                    contentManager.SolidWhiteTexture)));
        }

        private void SetGlobalLightingBufferVS(GraphicsDevice graphicsDevice)
        {
            var cloudShadowView = Matrix4x4.CreateLookAt(
                Vector3.Zero,
                Vector3.Normalize(new Vector3(0, 0.2f, -1)),
                Vector3.UnitY);

            var cloudShadowProjection = Matrix4x4.CreateOrthographic(1, 1, 0, 1);

            var lightingConstantsVS = new GlobalLightingTypes.LightingConstantsVS
            {
                CloudShadowMatrix = cloudShadowView * cloudShadowProjection
            };

            using (var commandList = graphicsDevice.ResourceFactory.CreateCommandList())
            {
                commandList.Begin();

                _globalLightingBufferVS.Value = lightingConstantsVS;
                _globalLightingBufferVS.Update(commandList);

                commandList.End();

                graphicsDevice.SubmitCommands(commandList);
            }
        }

        public void UpdateGlobalConstantBuffers(
            CommandList commandList,
            in Matrix4x4 viewProjection)
        {
            _globalConstantBufferVS.Value.ViewProjection = viewProjection;
            _globalConstantBufferVS.Update(commandList);
        }

        public void UpdateStandardPassConstantBuffers(
            CommandList commandList,
            RenderContext context)
        {
            var cameraPosition = Matrix4x4Utility.Invert(context.Scene3D.Camera.View).Translation;
            _globalConstantBufferShared.Value.CameraPosition = cameraPosition;
            _globalConstantBufferShared.Value.TimeInSeconds = (float) context.GameTime.TotalGameTime.TotalSeconds;
            _globalConstantBufferShared.Update(commandList);

            var viewportSize = new Vector2(context.RenderTarget.Width, context.RenderTarget.Height);
            if (viewportSize != _globalConstantBufferPS.Value.ViewportSize)
            {
                _globalConstantBufferPS.Value.ViewportSize = viewportSize;
                _globalConstantBufferPS.Update(commandList);
            }

            if (_cachedTimeOfDay != context.Scene3D.Lighting.TimeOfDay)
            {
                _globalLightingTerrainBufferPS.Value = context.Scene3D.Lighting.CurrentLightingConfiguration.TerrainLightsPS;
                _globalLightingTerrainBufferPS.Update(commandList);

                _globalLightingObjectBufferPS.Value = context.Scene3D.Lighting.CurrentLightingConfiguration.ObjectLightsPS;
                _globalLightingObjectBufferPS.Update(commandList);

                _cachedTimeOfDay = context.Scene3D.Lighting.TimeOfDay;
            }
        }
    }
}
