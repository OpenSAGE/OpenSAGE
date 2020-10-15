using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class GlobalShaderResourceData : DisposableBase
    {
        private readonly ConstantBuffer<GlobalShaderResources.GlobalConstantsShared> _globalConstantBufferShared;
        private readonly ConstantBuffer<GlobalShaderResources.GlobalConstantsVS> _globalConstantBufferVS;
        private readonly ConstantBuffer<GlobalShaderResources.GlobalConstantsPS> _globalConstantBufferPS;

        private readonly ConstantBuffer<GlobalShaderResources.LightingConstantsVS> _globalLightingBufferVS;
        private readonly ConstantBuffer<GlobalShaderResources.LightingConstantsPS> _globalLightingTerrainBufferPS;
        private readonly ConstantBuffer<GlobalShaderResources.LightingConstantsPS> _globalLightingObjectBufferPS;

        public readonly ResourceSet GlobalConstantsResourceSet;
        public readonly ResourceSet GlobalLightingConstantsTerrainResourceSet;
        public readonly ResourceSet GlobalLightingConstantsObjectResourceSet;

        private TimeOfDay? _cachedTimeOfDay;

        public GlobalShaderResourceData(
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources)
        {
            _globalConstantBufferShared = AddDisposable(new ConstantBuffer<GlobalShaderResources.GlobalConstantsShared>(graphicsDevice, "GlobalConstantsShared"));
            _globalConstantBufferVS = AddDisposable(new ConstantBuffer<GlobalShaderResources.GlobalConstantsVS>(graphicsDevice, "GlobalConstantsVS"));
            _globalConstantBufferPS = AddDisposable(new ConstantBuffer<GlobalShaderResources.GlobalConstantsPS>(graphicsDevice, "GlobalConstantsPS"));

            GlobalConstantsResourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    globalShaderResources.GlobalConstantsResourceLayout,
                    _globalConstantBufferShared.Buffer,
                    _globalConstantBufferVS.Buffer,
                    _globalConstantBufferPS.Buffer)));

            _globalLightingBufferVS = AddDisposable(new ConstantBuffer<GlobalShaderResources.LightingConstantsVS>(graphicsDevice, "GlobalLightingConstantsVS (terrain)"));
            SetGlobalLightingBufferVS(graphicsDevice);

            _globalLightingTerrainBufferPS = AddDisposable(new ConstantBuffer<GlobalShaderResources.LightingConstantsPS>(graphicsDevice, "GlobalLightingConstantsPS (terrain)"));
            _globalLightingObjectBufferPS = AddDisposable(new ConstantBuffer<GlobalShaderResources.LightingConstantsPS>(graphicsDevice, "GlobalLightingConstantsPS (objects)"));

            GlobalLightingConstantsTerrainResourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    globalShaderResources.GlobalLightingConstantsResourceLayout,
                    _globalLightingBufferVS.Buffer,
                    _globalLightingTerrainBufferPS.Buffer)));

            GlobalLightingConstantsObjectResourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    globalShaderResources.GlobalLightingConstantsResourceLayout,
                    _globalLightingBufferVS.Buffer,
                    _globalLightingObjectBufferPS.Buffer)));
        }

        private void SetGlobalLightingBufferVS(GraphicsDevice graphicsDevice)
        {
            var cloudShadowView = Matrix4x4.CreateLookAt(
                Vector3.Zero,
                Vector3.Normalize(new Vector3(0, 0.2f, -1)),
                Vector3.UnitY);

            var cloudShadowProjection = Matrix4x4.CreateOrthographic(1, 1, 0, 1);

            var lightingConstantsVS = new GlobalShaderResources.LightingConstantsVS
            {
                CloudShadowMatrix = cloudShadowView * cloudShadowProjection
            };

            _globalLightingBufferVS.Value = lightingConstantsVS;
            _globalLightingBufferVS.Update(graphicsDevice);
        }

        public void UpdateGlobalConstantBuffers(
            CommandList commandList,
            in Matrix4x4 viewProjection,
            in Vector4? clippingPlane1,
            in Vector4? clippingPlane2)
        {
            _globalConstantBufferVS.Value.ViewProjection = viewProjection;

            _globalConstantBufferVS.Value.ClippingPlane1 = clippingPlane1 ?? Vector4.Zero;
            _globalConstantBufferVS.Value.ClippingPlane2 = clippingPlane2 ?? Vector4.Zero;

            _globalConstantBufferVS.Value.HasClippingPlane1 = clippingPlane1 != null;
            _globalConstantBufferVS.Value.HasClippingPlane2 = clippingPlane2 != null;

            _globalConstantBufferVS.Update(commandList);
        }

        public void UpdateStandardPassConstantBuffers(
            CommandList commandList,
            RenderContext context)
        {
            var cameraPosition = Matrix4x4Utility.Invert(context.Scene3D.Camera.View).Translation;
            _globalConstantBufferShared.Value.CameraPosition = cameraPosition;
            _globalConstantBufferShared.Value.TimeInSeconds = (float) context.GameTime.TotalTime.TotalSeconds;
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
