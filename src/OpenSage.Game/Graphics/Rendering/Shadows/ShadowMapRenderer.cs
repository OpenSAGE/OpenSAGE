﻿using System;
using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering.Shadows
{
    internal sealed class ShadowMapRenderer : DisposableBase
    {
        private readonly ShadowFrustumCalculator _shadowFrustumCalculator;
        private readonly BoundingFrustum _lightFrustum;
        private readonly GlobalShaderResources _globalShaderResources;

        private ResourceSet _resourceSet;

        private ShadowData _shadowData;

        private readonly ConstantBuffer<GlobalShaderResources.ShadowConstantsPS> _shadowConstantsPSBuffer;
        private GlobalShaderResources.ShadowConstantsPS _shadowConstants;

        public Texture ShadowMap => _shadowData?.ShadowMap;

        public ResourceSet ResourceSetForRendering => _resourceSet;

        public ShadowMapRenderer(
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources)
        {
            _shadowFrustumCalculator = new ShadowFrustumCalculator();
            _lightFrustum = new BoundingFrustum(Matrix4x4.Identity);

            _globalShaderResources = globalShaderResources;

            _shadowConstantsPSBuffer = AddDisposable(new ConstantBuffer<GlobalShaderResources.ShadowConstantsPS>(
                graphicsDevice,
                "ShadowConstantsPS"));
        }

        public void RenderShadowMap(
            Scene3D scene,
            GraphicsDevice graphicsDevice,
            CommandList commandList,
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
                || _shadowData.FarPlaneDistance != scene.Camera.FarPlaneDistance
                || _shadowData.NumSplits != numCascades))
            {
                RemoveAndDispose(ref _shadowData);
                RemoveAndDispose(ref _resourceSet);
            }

            if (_shadowData == null)
            {
                _shadowData = AddDisposable(new ShadowData(
                    numCascades,
                    scene.Camera.NearPlaneDistance,
                    scene.Camera.FarPlaneDistance,
                    shadowMapSize,
                    graphicsDevice));

                _resourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(
                        _globalShaderResources.GlobalShadowResourceLayout,
                        _shadowConstantsPSBuffer.Buffer,
                        _shadowData.ShadowMap,
                        _globalShaderResources.ShadowSampler)));
            }

            if (scene.Shadows.ShadowsType != ShadowsType.None)
            {
                _shadowFrustumCalculator.CalculateShadowData(
                    light,
                    scene.Camera,
                    _shadowData,
                    scene.Shadows);

                _shadowConstants.Set(numCascades, scene.Shadows, _shadowData);

                // Render scene geometry to each split of the cascade.
                for (var splitIndex = 0; splitIndex < _shadowData.NumSplits; splitIndex++)
                {
                    _lightFrustum.Matrix = _shadowData.ShadowCameraViewProjections[splitIndex];

                    drawSceneCallback(
                        _shadowData.ShadowMapFramebuffers[splitIndex],
                        _lightFrustum);
                }
            }
            else
            {
                _shadowConstants.ShadowsType = ShadowsType.None;
            }

            _shadowConstantsPSBuffer.Value = _shadowConstants;
            _shadowConstantsPSBuffer.Update(commandList);
        }
    }
}
