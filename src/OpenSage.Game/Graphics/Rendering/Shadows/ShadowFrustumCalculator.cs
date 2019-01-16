using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering.Shadows
{
    internal sealed class ShadowFrustumCalculator
    {
        private readonly Vector3[] _frustumCorners = new Vector3[8];
        private readonly float[] _cascadeSplits = new float[4];

        /// <summary>
        /// Determines the size of the frustum needed to cover a particular subsection of the viewable area,
        /// then creates an appropriate orthographic projection.
        /// </summary>
        /// <param name="shadowMapSize"></param>
        /// <param name="light">The directional light to use.</param>
        /// <param name="camera">The camera viewing the scene.</param>
        /// <param name="shadowData"></param>
        public void CalculateShadowData(
            in GlobalLightingTypes.Light light,
            Camera camera,
            ShadowData shadowData,
            ShadowSettings settings)
        {
            var originalFarPlaneDistance = camera.FarPlaneDistance;
            try
            {
                if (settings.ShadowDistance < camera.FarPlaneDistance)
                {
                    camera.FarPlaneDistance = settings.ShadowDistance;
                }

                var globalShadowMatrix = MakeGlobalShadowMatrix(light, camera, settings);
                shadowData.ShadowMatrix = globalShadowMatrix;

                switch (settings.ShadowMapCascades)
                {
                    case ShadowMapCascades.OneCascade:
                        _cascadeSplits[0] = 1.0f;
                        _cascadeSplits[1] = 0.0f;
                        _cascadeSplits[2] = 0.0f;
                        _cascadeSplits[3] = 0.0f;
                        break;

                    case ShadowMapCascades.TwoCascades:
                        _cascadeSplits[0] = settings.ShadowCascades2SplitDepth;
                        _cascadeSplits[1] = 1.0f;
                        _cascadeSplits[2] = 0.0f;
                        _cascadeSplits[3] = 0.0f;
                        break;

                    case ShadowMapCascades.FourCascades:
                        _cascadeSplits[0] = settings.ShadowCascades4SplitDepths.X;
                        _cascadeSplits[1] = settings.ShadowCascades4SplitDepths.Y;
                        _cascadeSplits[2] = settings.ShadowCascades4SplitDepths.Z;
                        _cascadeSplits[3] = 1.0f;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // Render the meshes to each cascade.
                var numCascades = (int) settings.ShadowMapCascades;
                for (var cascadeIdx = 0; cascadeIdx < numCascades; cascadeIdx++)
                {
                    // Get the 8 points of the view frustum in world space
                    ResetViewFrustumCorners();

                    var prevSplitDist = cascadeIdx == 0 ? 0.0f : _cascadeSplits[cascadeIdx - 1];
                    var splitDist = _cascadeSplits[cascadeIdx];

                    var invViewProj = Matrix4x4Utility.Invert(camera.ViewProjection);
                    for (var i = 0; i < 8; ++i)
                        _frustumCorners[i] = Vector4.Transform(_frustumCorners[i], invViewProj).ToVector3();

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
                        frustumCenter = frustumCenter + _frustumCorners[i];
                    frustumCenter /= 8.0f;

                    // Pick the up vector to use for the light camera
                    var upDir = camera.View.Right();

                    Vector3 minExtents;
                    Vector3 maxExtents;

                    if (settings.StabilizeShadowCascades)
                    {
                        // This needs to be constant for it to be stable
                        upDir = Vector3.UnitZ;

                        // Calculate the radius of a bounding sphere surrounding the frustum corners
                        var sphereRadius = 0.0f;
                        for (var i = 0; i < 8; ++i)
                        {
                            var dist = (_frustumCorners[i] - frustumCenter).Length();
                            sphereRadius = Math.Max(sphereRadius, dist);
                        }

                        sphereRadius = (float) Math.Ceiling(sphereRadius * 16.0f) / 16.0f;

                        maxExtents = new Vector3(sphereRadius);
                        minExtents = -maxExtents;
                    }
                    else
                    {
                        // Create a temporary view matrix for the light
                        var lightCameraPos = frustumCenter;
                        var lookAt = frustumCenter + light.Direction;
                        var lightView = Matrix4x4.CreateLookAt(lightCameraPos, lookAt, upDir);

                        // Calculate an AABB around the frustum corners
                        var mins = new Vector3(float.MaxValue);
                        var maxes = new Vector3(float.MinValue);
                        for (var i = 0; i < 8; ++i)
                        {
                            var corner = Vector4.Transform(_frustumCorners[i], lightView).ToVector3();
                            mins = Vector3.Min(mins, corner);
                            maxes = Vector3.Max(maxes, corner);
                        }

                        minExtents = mins;
                        maxExtents = maxes;

                        // Adjust the min/max to accommodate the filtering size
                        var kernelSize = GetFixedFilterKernelSize(settings.ShadowsType);
                        var scale = (shadowData.ShadowMapSize + kernelSize) / (float) shadowData.ShadowMapSize;
                        minExtents.X *= scale;
                        minExtents.Y *= scale;
                        maxExtents.X *= scale;
                        maxExtents.Y *= scale;
                    }

                    var cascadeExtents = maxExtents - minExtents;

                    // Get position of the shadow camera
                    var shadowCameraPos = frustumCenter + -light.Direction * -minExtents.Z;

                    // Come up with a new orthographic camera for the shadow caster
                    CreateOrthographicCamera(
                        minExtents.X, minExtents.Y, maxExtents.X, maxExtents.Y,
                        0.0f, cascadeExtents.Z,
                        shadowCameraPos, frustumCenter, upDir,
                        out var shadowCameraView, out var shadowCameraProjection);

                    if (settings.StabilizeShadowCascades)
                    {
                        // Create the rounding matrix, by projecting the world-space origin and determining
                        // the fractional offset in texel space
                        var shadowMatrixTemp = shadowCameraView * shadowCameraProjection;
                        var shadowOrigin = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                        shadowOrigin = Vector4.Transform(shadowOrigin, shadowMatrixTemp);
                        shadowOrigin = shadowOrigin * (shadowData.ShadowMapSize / 2.0f);

                        var roundedOrigin = Vector4Utility.Round(shadowOrigin);
                        var roundOffset = roundedOrigin - shadowOrigin;
                        roundOffset = roundOffset * (2.0f / shadowData.ShadowMapSize);
                        roundOffset.Z = 0.0f;
                        roundOffset.W = 0.0f;

                        var shadowProj = shadowCameraProjection;
                        //shadowProj.r[3] = shadowProj.r[3] + roundOffset;
                        shadowProj.M41 += roundOffset.X;
                        shadowProj.M42 += roundOffset.Y;
                        shadowProj.M43 += roundOffset.Z;
                        shadowProj.M44 += roundOffset.W;
                        shadowCameraProjection = shadowProj;
                    }

                    var shadowCameraViewProjection = shadowCameraView * shadowCameraProjection;
                    shadowData.ShadowCameraViewProjections[cascadeIdx] = shadowCameraViewProjection;

                    // Apply the scale/offset matrix, which transforms from [-1,1]
                    // post-projection space to [0,1] UV space
                    var texScaleBias =
                        Matrix4x4.CreateScale(0.5f, -0.5f, 1.0f) *
                        Matrix4x4.CreateTranslation(0.5f, 0.5f, 0.0f);
                    var shadowMatrix = shadowCameraViewProjection;
                    shadowMatrix = shadowMatrix * texScaleBias;

                    // Store the split distance in terms of view space depth
                    var clipDist = camera.FarPlaneDistance - camera.NearPlaneDistance;

                    shadowData.CascadeSplits[cascadeIdx] = camera.NearPlaneDistance + splitDist * clipDist;

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
                    shadowData.CascadeOffsets[cascadeIdx] = new Vector4(-cascadeCorner, 0.0f);
                    shadowData.CascadeScales[cascadeIdx] = new Vector4(cascadeScale, 1.0f);
                }
            }
            finally
            {
                camera.FarPlaneDistance = originalFarPlaneDistance;
            }
        }

        private static int GetFixedFilterKernelSize(ShadowsType shadowsType)
        {
            switch (shadowsType)
            {
                case ShadowsType.Hard:
                    return 2;

                case ShadowsType.Soft:
                    return 5;

                default:
                    throw new ArgumentOutOfRangeException(nameof(shadowsType));
            }
        }

        /// <summary>
        /// Makes the "global" shadow matrix used as the reference point for the cascades.
        /// </summary>
        private Matrix4x4 MakeGlobalShadowMatrix(in GlobalLightingTypes.Light light, Camera camera, ShadowSettings settings)
        {
            // Get the 8 points of the view frustum in world space
            ResetViewFrustumCorners();

            var invViewProj = Matrix4x4Utility.Invert(camera.View * camera.Projection);
            var frustumCenter = Vector3.Zero;
            for (var i = 0; i < 8; i++)
            {
                _frustumCorners[i] = Vector4.Transform(_frustumCorners[i], invViewProj).ToVector3();
                frustumCenter += _frustumCorners[i];
            }

            frustumCenter /= 8.0f;

            // Pick the up vector to use for the light camera
            var upDir = camera.View.Right();

            // This needs to be constant for it to be stable
            if (settings.StabilizeShadowCascades)
                upDir = Vector3.UnitZ;

            // Get position of the shadow camera
            var shadowCameraPos = frustumCenter + -light.Direction * -0.5f;

            // Come up with a new orthographic camera for the shadow caster
            CreateOrthographicCamera(-0.5f, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f,
                shadowCameraPos, frustumCenter, upDir,
                out var shadowCameraView, out var shadowCameraProjection);

            var texScaleBias = Matrix4x4.CreateScale(0.5f, -0.5f, 1.0f);
            texScaleBias.Translation = new Vector3(0.5f, 0.5f, 0.0f);
            return shadowCameraView * shadowCameraProjection * texScaleBias;
        }

        private static void CreateOrthographicCamera(
            float minX, float minY, float maxX, float maxY, float nearZ, float farZ,
            in Vector3 cameraPosition, in Vector3 cameraTarget, in Vector3 up,
            out Matrix4x4 view, out Matrix4x4 projection)
        {
            projection = Matrix4x4.CreateOrthographicOffCenter(minX, maxX, minY, maxY, nearZ, farZ);
            view = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, up);
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
