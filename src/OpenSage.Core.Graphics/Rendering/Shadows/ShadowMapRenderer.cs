using System;
using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui;
using OpenSage.Gui.DebugUI;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering.Shadows
{
    public sealed class ShadowMapRenderer : DisposableBase
    {
        private readonly ShadowFrustumCalculator _shadowFrustumCalculator;
        private readonly BoundingFrustum _lightFrustum;

        private ShadowData _shadowData;

        private readonly ConstantBuffer<GlobalShaderResources.ShadowConstantsPS> _shadowConstantsPSBuffer;
        private GlobalShaderResources.ShadowConstantsPS _shadowConstants;

        public Texture ShadowMap => _shadowData?.ShadowMap;

        public ConstantBuffer<GlobalShaderResources.ShadowConstantsPS> ShadowConstantsPSBuffer => _shadowConstantsPSBuffer;

        public ShadowMapRenderer(GraphicsDevice graphicsDevice)
        {
            _shadowFrustumCalculator = new ShadowFrustumCalculator();
            _lightFrustum = new BoundingFrustum(Matrix4x4.Identity);

            _shadowConstantsPSBuffer = AddDisposable(new ConstantBuffer<GlobalShaderResources.ShadowConstantsPS>(
                graphicsDevice,
                "ShadowConstantsPS"));
        }

        public void RenderShadowMap(
            in GlobalShaderResources.Light light,
            ShadowSettings settings,
            Cameras.Camera camera,
            GraphicsDevice graphicsDevice,
            CommandList commandList,
            Action<Framebuffer, BoundingFrustum> drawSceneCallback)
        {
            // Calculate size of shadow map.
            var shadowMapSize = settings.ShadowMapSize;
            var numCascades = (uint)settings.ShadowMapCascades;

            if (_shadowData != null && _shadowData.ShadowMap != null
                && (_shadowData.ShadowMap.Width != shadowMapSize
                || _shadowData.ShadowMap.Height != shadowMapSize
                || _shadowData.NearPlaneDistance != camera.NearPlaneDistance
                || _shadowData.FarPlaneDistance != camera.FarPlaneDistance
                || _shadowData.NumSplits != numCascades))
            {
                RemoveAndDispose(ref _shadowData);
            }

            if (_shadowData == null)
            {
                _shadowData = AddDisposable(new ShadowData(
                    numCascades,
                    camera.NearPlaneDistance,
                    camera.FarPlaneDistance,
                    shadowMapSize,
                    graphicsDevice));
            }

            if (settings.ShadowsType != ShadowsType.None)
            {
                _shadowFrustumCalculator.CalculateShadowData(
                    light,
                    camera,
                    _shadowData,
                    settings);

                _shadowConstants.Set(numCascades, settings, _shadowData);

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

        private static readonly ColorRgbaF[] CascadeColors =
        {
            new ColorRgbaF(1, 0, 0, 1),
            new ColorRgbaF(0, 1, 0, 1),
            new ColorRgbaF(0, 0, 1, 1),
            new ColorRgbaF(1, 1, 1, 1),
        };

        public void DrawDebugOverlay(
            Cameras.Camera camera,
            DrawingContext2D drawingContext)
        {
            for (var splitIndex = 0; splitIndex < _shadowData.NumSplits; splitIndex++)
            {
                _lightFrustum.Matrix = _shadowData.ShadowCameraViewProjections[splitIndex];

                var corners = _lightFrustum.Corners;
                var color = CascadeColors[splitIndex];

                void DrawLine(int start, int end)
                {
                    DebugDrawingUtils.DrawLine(drawingContext, camera, corners[start], corners[end], color);
                }

                DrawLine(0, 1);
                DrawLine(1, 2);
                DrawLine(2, 3);
                DrawLine(3, 0);

                DrawLine(4, 5);
                DrawLine(5, 6);
                DrawLine(6, 7);
                DrawLine(7, 4);

                DrawLine(0, 4);
                DrawLine(1, 5);
                DrawLine(2, 6);
                DrawLine(3, 7);
            }
        }
    }
}
