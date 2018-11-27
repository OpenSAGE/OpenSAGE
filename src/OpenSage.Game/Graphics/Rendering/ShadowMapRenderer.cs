using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class ShadowMapRenderer
    {
        public const uint ShadowMapSize = 1024;
        public const uint NumCascades = 4;

        private readonly ShadowSettings _settings;

        private readonly float[] _cascadeSplits;
        private readonly Vector3[] _frustumCorners;

        public ShadowMapRenderer(ShadowSettings settings)
        {
            _settings = settings;

            _cascadeSplits = new float[NumCascades];
            _frustumCorners = new Vector3[8];
        }

        public void RenderShadowMap(
            Camera camera,
            in Vector3 lightDirection,
            ref ShadowConstantsPS constants,
            Action<uint, Camera> drawCallback)
        {
            // Set cascade split ratios.
            _cascadeSplits[0] = _settings.SplitDistance0;
            _cascadeSplits[1] = _settings.SplitDistance1;
            _cascadeSplits[2] = _settings.SplitDistance2;
            _cascadeSplits[3] = _settings.SplitDistance3;

            var globalShadowMatrix = MakeGlobalShadowMatrix(camera.ViewProjection, lightDirection);
            constants.ShadowMatrix = globalShadowMatrix;

            // Render the meshes to each cascade.
            for (var cascadeIdx = 0u; cascadeIdx < NumCascades; ++cascadeIdx)
            {
                // Get the 8 points of the view frustum in world space
                ResetViewFrustumCorners();

                var prevSplitDist = cascadeIdx == 0 ? 0.0f : _cascadeSplits[cascadeIdx - 1];
                var splitDist = _cascadeSplits[cascadeIdx];

                var invViewProj = Matrix4x4Utility.Invert(camera.ViewProjection);
                for (var i = 0; i < 8; ++i)
                {
                    _frustumCorners[i] = Vector4.Transform(_frustumCorners[i], invViewProj).ToVector3();
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

                // Up vector for light camera needs to be constant for it to be stable
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
                var shadowCameraPos = frustumCenter - lightDirection * -minExtents.Z;

                // Come up with a new orthographic camera for the shadow caster
                var shadowCamera = new OrthographicCamera(
                    minExtents.X, minExtents.Y, maxExtents.X, maxExtents.Y,
                    0.0f, cascadeExtents.Z);
                shadowCamera.SetLookAt(shadowCameraPos, frustumCenter, upDir);

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
                shadowCamera.SetProjection(shadowProj);

                // Draw the mesh with depth only, using the new shadow camera
                drawCallback(cascadeIdx, shadowCamera);

                // Apply the scale/offset matrix, which transforms from [-1,1]
                // post-projection space to [0,1] UV space
                var texScaleBias = Matrix4x4.CreateScale(0.5f, -0.5f, 1.0f)
                    * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0.0f);
                var shadowMatrix = shadowCamera.ViewProjection;
                shadowMatrix = shadowMatrix * texScaleBias;

                // Store the split distance in terms of view space depth
                var clipDist = camera.FarPlaneDistance - camera.NearPlaneDistance;

                constants.SetCascadeSplit(cascadeIdx, camera.NearPlaneDistance + splitDist * clipDist);

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
                constants.SetCascadeOffset(cascadeIdx, new Vector4(-cascadeCorner, 0.0f));
                constants.SetCascadeScale(cascadeIdx, new Vector4(cascadeScale, 1.0f));
            }
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

            // Up vector needs to be constant for it to be stable
            var upDir = Vector3.UnitZ;

            // Get position of the shadow camera
            var shadowCameraPos = frustumCenter - lightDirection * -0.5f;

            // Come up with a new orthographic camera for the shadow caster
            var shadowCamera = new OrthographicCamera(-0.5f, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f);
            shadowCamera.SetLookAt(shadowCameraPos, frustumCenter, upDir);

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

    public struct ShadowConstantsPS
    {
        public Matrix4x4 ShadowMatrix;
        public float CascadeSplit0;
        public float CascadeSplit1;
        public float CascadeSplit2;
        public float CascadeSplit3;
        public Vector4 CascadeOffset0;
        public Vector4 CascadeOffset1;
        public Vector4 CascadeOffset2;
        public Vector4 CascadeOffset3;
        public Vector4 CascadeScale0;
        public Vector4 CascadeScale1;
        public Vector4 CascadeScale2;
        public Vector4 CascadeScale3;
        public float Bias;
        public float OffsetScale;
        public uint VisualizeCascades;
        public uint FilterAcrossCascades;
        Vector3 _Padding;
        public FixedFilterSize FilterSize;

        public void SetCascadeSplit(uint index, float value)
        {
            switch (index)
            {
                case 0:
                    CascadeSplit0 = value;
                    break;

                case 1:
                    CascadeSplit1 = value;
                    break;

                case 2:
                    CascadeSplit2 = value;
                    break;

                case 3:
                    CascadeSplit3 = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public void SetCascadeOffset(uint index, in Vector4 value)
        {
            switch (index)
            {
                case 0:
                    CascadeOffset0 = value;
                    break;

                case 1:
                    CascadeOffset1 = value;
                    break;

                case 2:
                    CascadeOffset2 = value;
                    break;

                case 3:
                    CascadeOffset3 = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public void SetCascadeScale(uint index, in Vector4 value)
        {
            switch (index)
            {
                case 0:
                    CascadeScale0 = value;
                    break;

                case 1:
                    CascadeScale1 = value;
                    break;

                case 2:
                    CascadeScale2 = value;
                    break;

                case 3:
                    CascadeScale3 = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
