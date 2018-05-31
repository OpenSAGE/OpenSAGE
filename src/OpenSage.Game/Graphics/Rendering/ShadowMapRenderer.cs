using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Settings;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class ShadowMapRenderer : DisposableBase
    {
        public const int ShadowMapSize = 1024;

        private readonly Texture _shadowMap;
        private readonly Framebuffer[] _shadowMapFramebuffers;

        private readonly Vector3[] _frustumCorners;

        public ShadowMapRenderer(GraphicsDevice graphicsDevice)
        {
            _shadowMap = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    ShadowMapSize,
                    ShadowMapSize,
                    1,
                    ShadowHelpers.NumCascades,
                    RenderPipeline.ShadowMapPixelFormat,
                    TextureUsage.DepthStencil | TextureUsage.Sampled)));

            _shadowMapFramebuffers = new Framebuffer[ShadowHelpers.NumCascades];
            for (var i = 0u; i < ShadowHelpers.NumCascades; i++)
            {
                _shadowMapFramebuffers[i] = AddDisposable(graphicsDevice.ResourceFactory.CreateFramebuffer(
                    new FramebufferDescription(
                        new FramebufferAttachmentDescription(_shadowMap, i),
                        Array.Empty<FramebufferAttachmentDescription>())));
            }

            _frustumCorners = new Vector3[8];
        }

        public void RenderShadowMaps(
            CommandList commandList,
            WorldLighting lightingSettings,
            PerspectiveCamera sceneCamera,
            ref ShadowHelpers.Global_ShadowConstantsPS shadowConstants,
            out Texture shadowMap,
            Action<Camera> doRenderPass)
        {
            shadowConstants.ShadowMapSize = ShadowMapSize;
            shadowConstants.FilterAcrossCascades = lightingSettings.ShadowSettings.VisualizeCascades ? 1u : 0u;
            shadowConstants.VisualizeCascades = lightingSettings.ShadowSettings.VisualizeCascades ? 1u : 0u;
            shadowConstants.FilterSize = lightingSettings.ShadowSettings.FilterSize;
            shadowConstants.Bias = lightingSettings.ShadowSettings.Bias;
            shadowConstants.OffsetScale = lightingSettings.ShadowSettings.OffsetScale;

            for (var i = 0; i < ShadowHelpers.NumCascades; i++)
            {
                shadowConstants.CascadeSplits[i] = lightingSettings.ShadowSettings.SplitDistances[i];
            }

            var sceneViewProjection = sceneCamera.ViewProjection;
            var invViewProjection = Matrix4x4Utility.Invert(sceneViewProjection);

            var lightDirection = lightingSettings.CurrentLightingConfiguration.TerrainLightsPS.Light0.Direction;

            var globalShadowMatrix = MakeGlobalShadowMatrix(sceneViewProjection, lightDirection);

            shadowConstants.ShadowMatrix = globalShadowMatrix;

            for (var cascadeIdx = 0u; cascadeIdx < ShadowHelpers.NumCascades; cascadeIdx++)
            {
                commandList.SetFramebuffer(_shadowMapFramebuffers[cascadeIdx]);
                commandList.ClearDepthStencil(1);
                commandList.SetFullViewports();

                // Get the 8 points of the view frustum in world space
                ResetViewFrustumCorners();

                var prevSplitDist = cascadeIdx == 0 ? 0.0f : shadowConstants.CascadeSplits[cascadeIdx - 1];
                var splitDist = shadowConstants.CascadeSplits[cascadeIdx];

                for (var i = 0; i < 8; i++)
                {
                    _frustumCorners[i] = Vector4.Transform(_frustumCorners[i], invViewProjection).ToVector3();
                }

                // Get the corners of the current cascade slice of the view frustum
                for (var i = 0; i < 4; ++i)
                {
                    var cornerRay = _frustumCorners[i + 4] - _frustumCorners[i];
                    var nearCornerRay = cornerRay * prevSplitDist;
                    var farCornerRay = cornerRay * splitDist;
                    _frustumCorners[i + 4] = _frustumCorners[i] + farCornerRay;
                    _frustumCorners[i] = _frustumCorners[i] + nearCornerRay;
                }

                // Calculate the centroid of the view frustum slice
                var frustumCenter = Vector3.Zero;
                for (var i = 0; i < 8; ++i)
                {
                    frustumCenter = frustumCenter + _frustumCorners[i];
                }
                frustumCenter /= 8.0f;

                // This needs to be constant for it to be stable
                var upDir = Vector3.UnitZ;

                // Calculate the radius of a bounding sphere surrounding the frustum corners
                var sphereRadius = 0.0f;
                for (var i = 0; i < 8; ++i)
                {
                    var dist = (_frustumCorners[i] - frustumCenter).Length();
                    sphereRadius = Math.Max(sphereRadius, dist);
                }

                sphereRadius = (float) Math.Ceiling(sphereRadius * 16.0f) / 16.0f;

                var maxExtents = new Vector3(sphereRadius);
                var minExtents = -maxExtents;

                var cascadeExtents = maxExtents - minExtents;

                // Get position of the shadow camera
                var shadowCameraPos = frustumCenter + lightDirection * -minExtents.Z;

                // Come up with a new orthographic camera for the shadow caster
                var shadowCamera = new OrthographicCamera(
                    minExtents.X, minExtents.Y, maxExtents.X, maxExtents.Y,
                    0.0f, cascadeExtents.Z);
                shadowCamera.SetLookAt(shadowCameraPos, frustumCenter, upDir);

                // Stabilise cascades

                // Create the rounding matrix, by projecting the world-space origin and determining
                // the fractional offset in texel space
                var shadowMatrixTemp = shadowCamera.ViewProjection;
                var shadowOrigin = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                shadowOrigin = Vector4.Transform(shadowOrigin, shadowMatrixTemp);
                shadowOrigin = shadowOrigin * (ShadowMapSize / 2.0f);

                var roundedOrigin = Vector4Utility.Round(shadowOrigin);
                var roundOffset = roundedOrigin - shadowOrigin;
                roundOffset = roundOffset * (2.0f / ShadowMapSize);
                roundOffset.Z = 0.0f;
                roundOffset.W = 0.0f;

                var shadowProj = shadowCamera.Projection;
                //shadowProj.r[3] = shadowProj.r[3] + roundOffset;
                shadowProj.M41 += roundOffset.X;
                shadowProj.M42 += roundOffset.Y;
                shadowProj.M43 += roundOffset.Z;
                shadowProj.M44 += roundOffset.W;
                shadowCamera.Projection = shadowProj;

                doRenderPass(shadowCamera);

                // Apply the scale/offset matrix, which transforms from [-1,1]
                // post-projection space to [0,1] UV space
                var texScaleBias = Matrix4x4.CreateScale(0.5f, -0.5f, 1.0f)
                    * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0.0f);
                var shadowMatrix = shadowCamera.ViewProjection;
                shadowMatrix = shadowMatrix * texScaleBias;

                // Store the split distance in terms of view space depth
                var clipDist = sceneCamera.FarPlaneDistance - sceneCamera.NearPlaneDistance;

                shadowConstants.CascadeSplits[cascadeIdx] = sceneCamera.NearPlaneDistance + splitDist * clipDist;

                // Calculate the position of the lower corner of the cascade partition, in the UV space
                // of the first cascade partition
                var invCascadeMat = Matrix4x4Utility.Invert(shadowMatrix);
                var cascadeCorner = Vector4.Transform(Vector3.Zero, invCascadeMat).ToVector3();
                cascadeCorner = Vector4.Transform(cascadeCorner, globalShadowMatrix).ToVector3();

                // Do the same for the upper corner
                var otherCorner = Vector4.Transform(Vector3.One, invCascadeMat).ToVector3();
                otherCorner = Vector4.Transform(otherCorner, globalShadowMatrix).ToVector3();

                // Calculate the scale and offset
                var cascadeScale = Vector3.One / (otherCorner - cascadeCorner);
                shadowConstants.CascadeOffsets[cascadeIdx] = new Vector4(-cascadeCorner, 0.0f);
                shadowConstants.CascadeScales[cascadeIdx] = new Vector4(cascadeScale, 1.0f);
            }

            shadowMap = _shadowMap;
        }

        /// <summary>
        /// Makes the "global" shadow matrix used as the reference point for the cascades.
        /// </summary>
        private Matrix4x4 MakeGlobalShadowMatrix(in Matrix4x4 cameraViewProjection, in Vector3 lightDirection)
        {
            // Get the 8 points of the view frustum in world space
            ResetViewFrustumCorners();

            var invViewProj = Matrix4x4Utility.Invert(cameraViewProjection);
            var frustumCenter = Vector3.Zero;
            for (var i = 0; i < 8; i++)
            {
                _frustumCorners[i] = Vector4.Transform(_frustumCorners[i], invViewProj).ToVector3();
                frustumCenter += _frustumCorners[i];
            }

            frustumCenter /= 8.0f;

            // Get position of the shadow camera
            var shadowCameraPos = frustumCenter + lightDirection * -0.5f;

            // Come up with a new orthographic camera for the shadow caster
            var shadowCamera = new OrthographicCamera(-0.5f, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f);
            shadowCamera.SetLookAt(shadowCameraPos, frustumCenter, Vector3.UnitZ);

            var texScaleBias = Matrix4x4.CreateScale(0.5f, -0.5f, 1.0f);
            texScaleBias.Translation = new Vector3(0.5f, 0.5f, 0.0f);
            return shadowCamera.ViewProjection * texScaleBias;
        }

        private void ResetViewFrustumCorners()
        {
            _frustumCorners[0] = new Vector3(-1.0f, 1.0f, 0.0f);
            _frustumCorners[1] = new Vector3(1.0f, 1.0f, 0.0f);
            _frustumCorners[2] = new Vector3(1.0f, -1.0f, 0.0f);
            _frustumCorners[3] = new Vector3(-1.0f, -1.0f, 0.0f);
            _frustumCorners[4] = new Vector3(-1.0f, 1.0f, 1.0f);
            _frustumCorners[5] = new Vector3(1.0f, 1.0f, 1.0f);
            _frustumCorners[6] = new Vector3(1.0f, -1.0f, 1.0f);
            _frustumCorners[7] = new Vector3(-1.0f, -1.0f, 1.0f);
        }
    }
}
