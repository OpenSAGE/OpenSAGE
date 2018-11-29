using System;
using System.Numerics;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering.Shadows
{
    internal sealed class ShadowMapRenderer : DisposableBase
    {
        private readonly ShadowFrustumCalculator _shadowFrustumCalculator;
        private readonly BoundingFrustum _lightFrustum;

        private ShadowData _shadowData;

        public ShadowMapRenderer()
        {
            _shadowFrustumCalculator = new ShadowFrustumCalculator();
            _lightFrustum = new BoundingFrustum(Matrix4x4.Identity);
        }

        public void RenderShadowMap(
            Scene3D scene,
            GraphicsDevice graphicsDevice,
            ref ShadowConstantsPS constants,
            out Texture shadowMap,
            Action<Framebuffer, BoundingFrustum> drawSceneCallback)
        {
            // TODO: Use terrain light for terrain self-shadowing?
            var light = scene.Lighting.CurrentLightingConfiguration.ObjectLightsPS.Light0;

            // Calculate size of shadow map.
            var shadowMapSize = scene.Shadows.ShadowMapSize;
            var numCascades = (uint) scene.Shadows.ShadowMapCascades;

            if (_shadowData != null && _shadowData.ShadowMap != null
                && (_shadowData.ShadowMap.Width != shadowMapSize
                || _shadowData.ShadowMap.Height != shadowMapSize
                || _shadowData.NearPlaneDistance != scene.Camera.NearPlaneDistance
                || _shadowData.FarPlaneDistance != scene.Camera.FarPlaneDistance))
            {
                RemoveAndDispose(ref _shadowData);
            }

            if (_shadowData == null)
            {
                _shadowData = AddDisposable(new ShadowData(
                    numCascades,
                    scene.Camera.NearPlaneDistance,
                    scene.Camera.FarPlaneDistance,
                    shadowMapSize,
                    graphicsDevice));
            }

            _shadowFrustumCalculator.CalculateShadowData(
                light,
                scene.Camera,
                _shadowData,
                scene.Shadows);

            constants.Set(numCascades, scene.Shadows, _shadowData);

            // Render scene geometry to each split of the cascade.
            for (var splitIndex = 0; splitIndex < _shadowData.NumSplits; splitIndex++)
            {
                _lightFrustum.Matrix = _shadowData.ShadowCameraViewProjections[splitIndex];

                drawSceneCallback(
                    _shadowData.ShadowMapFramebuffers[splitIndex],
                    _lightFrustum);
            }

            shadowMap = _shadowData.ShadowMap;
        }
    }
}
