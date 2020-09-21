using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Map;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering.Water
{
    internal sealed class WaterMapRenderer : DisposableBase
    {
        private ResourceSet _resourceSet;
        private WaterData _waterData;

        public Texture ReflectionMap => _waterData?.ReflectionMap;
        public Texture ReflectionDepthMap => _waterData?.ReflectionDepthMap;
        public Framebuffer ReflectionFramebuffer => _waterData?.ReflectionMapFramebuffer;

        public Texture RefractionMap => _waterData?.RefractionMap;
        public Texture RefractionDepthMap => _waterData?.RefractionDepthMap;
        public Framebuffer RefractionFramebuffer => _waterData?.RefractionMapFramebuffer;

        public ResourceSet ResourceSetForRendering => _resourceSet;

        private readonly Dictionary<TimeOfDay, Texture> _waterTextureSet;
        private readonly Dictionary<TimeOfDay, Vector2> _waterUvScrollSet;
        private readonly Dictionary<TimeOfDay, ColorRgba> _waterDiffuseColorSet;
        private readonly Dictionary<TimeOfDay, ColorRgba> _waterTransparentDiffuseColorSet;
        private readonly Texture _bumpTexture;

        private ConstantBuffer<GlobalShaderResources.WaterConstantsPS> _waterConstantsPSBuffer;
        private GlobalShaderResources.WaterConstantsPS _waterConstantsPS;

        private readonly WaterShaderResources _waterShaderResources;

        private Vector2 _uvOffset;

        private DeltaTimer _deltaTimer;

        private float _farPlaneDistance;
        private float _nearPlaneDistance;

        private bool _isRenderReflection;
        private bool _isRenderRefraction;

        private readonly float _transparentWaterDepth;
        private readonly float _transparentWaterMinOpacity;

        public WaterMapRenderer(
            AssetStore assetStore,
            GraphicsLoadContext graphicsLoadContext,
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources)
        {
            _waterShaderResources = new WaterShaderResources(graphicsDevice, globalShaderResources);

            var _waterSets = assetStore.WaterSets;
            _waterTextureSet = new Dictionary<TimeOfDay, Texture>();
            _waterUvScrollSet = new Dictionary<TimeOfDay, Vector2>();
            _waterDiffuseColorSet = new Dictionary<TimeOfDay, ColorRgba>();
            _waterTransparentDiffuseColorSet = new Dictionary<TimeOfDay, ColorRgba>();

            foreach (var waterSet in _waterSets)
            {
                _waterTextureSet.Add(waterSet.TimeOfDay, waterSet.WaterTexture.Value.Texture);
                _waterUvScrollSet.Add(waterSet.TimeOfDay, new Vector2(waterSet.UScrollPerMS, waterSet.VScrollPerMS));
                _waterDiffuseColorSet.Add(waterSet.TimeOfDay, waterSet.DiffuseColor);
                _waterTransparentDiffuseColorSet.Add(waterSet.TimeOfDay, waterSet.TransparentDiffuseColor);
            }

            _bumpTexture = graphicsLoadContext.StandardGraphicsResources.SolidWhiteTexture;

            _waterConstantsPSBuffer = AddDisposable(new ConstantBuffer<GlobalShaderResources.WaterConstantsPS>(
                graphicsDevice,
                "WaterConstantsPS"));

            _uvOffset = Vector2.Zero;

            _transparentWaterDepth = assetStore.WaterTransparency.Current.TransparentWaterDepth;
            _transparentWaterMinOpacity = assetStore.WaterTransparency.Current.TransparentWaterMinOpacity;
        }

        private void UpdateTimer()
        {
            if (_deltaTimer == null)
            {
                _deltaTimer = new DeltaTimer();
                _deltaTimer.Start();
            }
            else
            {
                _deltaTimer.Update();
            }
        }

        private void CalculateUVOffset(TimeOfDay timeOfDay)
        {
            // UVScroll specifies the amount of pixels the texture moves per millisecond
            var deltaTime = (float) _deltaTimer.CurrentGameTime.DeltaTime.Milliseconds;
            var tex = _waterTextureSet[timeOfDay];
            var texSize = new Vector2(tex.Width, tex.Height);
            var uvScroll = _waterUvScrollSet[timeOfDay] * deltaTime / texSize;
            _uvOffset += uvScroll;

            if (_uvOffset.X >= 1)
            {
                _uvOffset.X %= 1;
            }

            if (_uvOffset.Y >= 1)
            {
                _uvOffset.Y %= 1;
            }
        }

        private void UpdateVariableBuffers(TimeOfDay timeOfDay)
        {
            _waterConstantsPS.UVOffset = _uvOffset;
            _waterConstantsPS.FarPlaneDistance = _farPlaneDistance;
            _waterConstantsPS.NearPlaneDistance = _nearPlaneDistance;
            _waterConstantsPS.IsRenderReflection = _isRenderReflection ? 1u : 0;
            _waterConstantsPS.IsRenderRefraction = _isRenderRefraction ? 1u : 0;
            _waterConstantsPS.TransparentWaterDepth = _transparentWaterDepth;
            _waterConstantsPS.TransparentWaterMinOpacity = _transparentWaterMinOpacity;
            _waterConstantsPS.DiffuseColor = _waterDiffuseColorSet[timeOfDay].ToColorRgbaF();
            _waterConstantsPS.TransparentDiffuseColor = _waterTransparentDiffuseColorSet[timeOfDay].ToColorRgbaF();
            _waterConstantsPSBuffer.Value = _waterConstantsPS;
        }

        public void RenderWaterShaders(
            Scene3D scene,
            GraphicsDevice graphicsDevice,
            CommandList commandList,
            Action<Framebuffer?, Framebuffer?> drawSceneCallback)
        {
            // TODO: Get bump texture from water area somehow
            //_bumpTexture = assetStore.Textures.GetByName(bumpTexName);

            var reflectionMapSize = scene.Waters.ReflectionMapSize;
            var refractionMapSize = scene.Waters.RefractionMapSize;

            _farPlaneDistance = scene.Camera.FarPlaneDistance;
            _nearPlaneDistance = scene.Camera.NearPlaneDistance;

            _isRenderReflection = scene.Waters.IsRenderReflection;
            _isRenderRefraction = scene.Waters.IsRenderRefraction;

            UpdateTimer();
            CalculateUVOffset(scene.Lighting.TimeOfDay);
            UpdateVariableBuffers(scene.Lighting.TimeOfDay);

            _waterConstantsPSBuffer.Update(commandList);

            if (_waterData != null && _waterData.ReflectionMap != null && _waterData.RefractionMap != null
                && (_waterData.ReflectionMapSize != reflectionMapSize
                || _waterData.RefractionMapSize != refractionMapSize))
            {
                RemoveAndDispose(ref _waterData);
                RemoveAndDispose(ref _resourceSet);
                _deltaTimer.Reset();
            }

            if (_waterData == null)
            {
                _waterData = AddDisposable(new WaterData(
                    reflectionMapSize,
                    refractionMapSize,
                    graphicsDevice));

                var texture = _waterTextureSet[scene.Lighting.TimeOfDay];

                _resourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(
                        _waterShaderResources.WaterResourceLayout,
                        _waterConstantsPSBuffer.Buffer,
                        texture,
                        _bumpTexture,
                        graphicsDevice.Aniso4xSampler,
                        _waterData.ReflectionMap,
                        graphicsDevice.LinearSampler,
                        _waterData.RefractionMap,
                        graphicsDevice.LinearSampler,
                        _waterData.RefractionDepthMap)));

            }

            // TODO: Fix soft edge water rendering when refraction is turned off
            if (!_isRenderReflection && !_isRenderRefraction)
            {
                drawSceneCallback(null, null);
            }
            else if (!_isRenderReflection)
            {
                drawSceneCallback(null, RefractionFramebuffer);
            }
            else if (!_isRenderRefraction)
            {
                drawSceneCallback(ReflectionFramebuffer, null);
            }
            else
            {
                drawSceneCallback(ReflectionFramebuffer, RefractionFramebuffer);
            }
        }
    }
}
