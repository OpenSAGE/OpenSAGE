using System.Numerics;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;

namespace OpenSage.Graphics.Shaders
{
    public static class ShadowHelpers
    {
        public const int NumCascades = 4;

        public struct Global_ShadowConstantsPS
        {
            public Matrix4x4 ShadowMatrix;

            [ArraySize(NumCascades)]
            public float[] CascadeSplits;

            [ArraySize(NumCascades)]
            public Vector4[] CascadeOffsets;

            [ArraySize(NumCascades)]
            public Vector4[] CascadeScales;

            public uint ShadowMapSize;
            public uint /*bool*/ FilterAcrossCascades;
            public uint /*bool*/ VisualizeCascades;
            public ShadowMapFilterSize FilterSize;

            public float Bias;
            public float OffsetScale;

            public void Initialize()
            {
                CascadeSplits = new float[NumCascades];
                CascadeOffsets = new Vector4[NumCascades];
                CascadeScales = new Vector4[NumCascades];
            }

            public Blittable GetBlittable()
            {
                return new Blittable
                {
                    ShadowMatrix = ShadowMatrix,
                    CascadeSplits = new Vector4(CascadeSplits[0], CascadeSplits[1], CascadeSplits[2], CascadeSplits[3]),
                    CascadeOffsets0 = CascadeOffsets[0],
                    CascadeOffsets1 = CascadeOffsets[1],
                    CascadeOffsets2 = CascadeOffsets[2],
                    CascadeOffsets3 = CascadeOffsets[3],
                    CascadeScales0 = CascadeScales[0],
                    CascadeScales1 = CascadeScales[1],
                    CascadeScales2 = CascadeScales[2],
                    CascadeScales3 = CascadeScales[3],
                    ShadowMapSize = ShadowMapSize,
                    FilterAcrossCascades = FilterAcrossCascades,
                    VisualizeCascades = VisualizeCascades,
                    FilterSize = FilterSize,
                    Bias = Bias,
                    OffsetScale = OffsetScale
                };
            }

            public struct Blittable
            {
                public Matrix4x4 ShadowMatrix;

                public Vector4 CascadeSplits;

                public Vector4 CascadeOffsets0;
                public Vector4 CascadeOffsets1;
                public Vector4 CascadeOffsets2;
                public Vector4 CascadeOffsets3;

                public Vector4 CascadeScales0;
                public Vector4 CascadeScales1;
                public Vector4 CascadeScales2;
                public Vector4 CascadeScales3;

                public uint ShadowMapSize;
                public uint /*bool*/ FilterAcrossCascades;
                public uint /*bool*/ VisualizeCascades;
                public ShadowMapFilterSize FilterSize;

                public float Bias;
                public float OffsetScale;
            }
        }

        public enum ShadowMapFilterSize : uint
        {
            Two,
            Three,
            Five,
            Seven
        }

        private static float SampleShadowMap(
            DepthTexture2DArrayResource shadowMap,
            SamplerComparisonResource shadowSampler,
            Vector2 baseUv,
            float u, float v,
            Vector2 shadowMapSizeInv,
            uint cascadeIdx,
            float depth)
        {
            var uv = baseUv + new Vector2(u, v) * shadowMapSizeInv;
            var z = depth;

            return SampleComparisonLevelZero(shadowMap, shadowSampler, uv, cascadeIdx, z);
        }

        private static float SampleShadowMapOptimizedPcf(
            Global_ShadowConstantsPS shadowConstants,
            DepthTexture2DArrayResource shadowMap,
            SamplerComparisonResource shadowSampler,
            Vector3 shadowPos,
            Vector3 shadowPosDX, Vector3 shadowPosDY,
            uint cascadeIdx)
        {
            var lightDepth = shadowPos.Z;

            lightDepth -= shadowConstants.Bias;

            var shadowMapSize = shadowConstants.ShadowMapSize;
            var uv = shadowPos.XY() * shadowMapSize; // 1 unit - 1 texel

            var shadowMapSizeInv = new Vector2(
                1.0f / shadowMapSize,
                1.0f / shadowMapSize);

            Vector2 baseUv;
            baseUv.X = Floor(uv.X + 0.5f);
            baseUv.Y = Floor(uv.Y + 0.5f);

            var s = (uv.X + 0.5f - baseUv.X);
            var t = (uv.Y + 0.5f - baseUv.Y);

            baseUv -= new Vector2(0.5f, 0.5f);
            baseUv *= shadowMapSizeInv;

            float sum = 0;

            switch (shadowConstants.FilterSize)
            {
                case ShadowMapFilterSize.Two:
                    return SampleComparisonLevelZero(shadowMap, shadowSampler, shadowPos.XY(), cascadeIdx, lightDepth);

                case ShadowMapFilterSize.Three:
                    {
                        var uw0 = (3 - 2 * s);
                        var uw1 = (1 + 2 * s);

                        var u0 = (2 - s) / uw0 - 1;
                        var u1 = s / uw1 + 1;

                        var vw0 = (3 - 2 * t);
                        var vw1 = (1 + 2 * t);

                        var v0 = (2 - t) / vw0 - 1;
                        var v1 = t / vw1 + 1;

                        sum += uw0 * vw0 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw1 * vw0 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw0 * vw1 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw1 * vw1 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth);

                        return sum * 1.0f / 16;
                    }

                case ShadowMapFilterSize.Five:
                    {
                        var uw0 = (4 - 3 * s);
                        var uw1 = 7;
                        var uw2 = (1 + 3 * s);

                        var u0 = (3 - 2 * s) / uw0 - 2;
                        var u1 = (3 + s) / uw1;
                        var u2 = s / uw2 + 2;

                        var vw0 = (4 - 3 * t);
                        var vw1 = 7;
                        var vw2 = (1 + 3 * t);

                        var v0 = (3 - 2 * t) / vw0 - 2;
                        var v1 = (3 + t) / vw1;
                        var v2 = t / vw2 + 2;

                        sum += uw0 * vw0 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw1 * vw0 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw2 * vw0 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u2, v0, shadowMapSizeInv, cascadeIdx, lightDepth);

                        sum += uw0 * vw1 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw1 * vw1 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw2 * vw1 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u2, v1, shadowMapSizeInv, cascadeIdx, lightDepth);

                        sum += uw0 * vw2 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u0, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw1 * vw2 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u1, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw2 * vw2 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u2, v2, shadowMapSizeInv, cascadeIdx, lightDepth);

                        return sum * 1.0f / 144;
                    }

                case ShadowMapFilterSize.Seven:
                default:
                    {
                        var uw0 = (5 * s - 6);
                        var uw1 = (11 * s - 28);
                        var uw2 = -(11 * s + 17);
                        var uw3 = -(5 * s + 1);

                        var u0 = (4 * s - 5) / uw0 - 3;
                        var u1 = (4 * s - 16) / uw1 - 1;
                        var u2 = -(7 * s + 5) / uw2 + 1;
                        var u3 = -s / uw3 + 3;

                        var vw0 = (5 * t - 6);
                        var vw1 = (11 * t - 28);
                        var vw2 = -(11 * t + 17);
                        var vw3 = -(5 * t + 1);

                        var v0 = (4 * t - 5) / vw0 - 3;
                        var v1 = (4 * t - 16) / vw1 - 1;
                        var v2 = -(7 * t + 5) / vw2 + 1;
                        var v3 = -t / vw3 + 3;

                        sum += uw0 * vw0 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw1 * vw0 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw2 * vw0 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u2, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw3 * vw0 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u3, v0, shadowMapSizeInv, cascadeIdx, lightDepth);

                        sum += uw0 * vw1 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw1 * vw1 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw2 * vw1 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u2, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw3 * vw1 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u3, v1, shadowMapSizeInv, cascadeIdx, lightDepth);

                        sum += uw0 * vw2 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u0, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw1 * vw2 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u1, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw2 * vw2 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u2, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw3 * vw2 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u3, v2, shadowMapSizeInv, cascadeIdx, lightDepth);

                        sum += uw0 * vw3 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u0, v3, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw1 * vw3 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u1, v3, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw2 * vw3 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u2, v3, shadowMapSizeInv, cascadeIdx, lightDepth);
                        sum += uw3 * vw3 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u3, v3, shadowMapSizeInv, cascadeIdx, lightDepth);

                        return sum * 1.0f / 2704;
                    }
            }
        }

        private static Vector3 SampleShadowCascade(
            Global_ShadowConstantsPS shadowConstants,
            DepthTexture2DArrayResource shadowMap,
            SamplerComparisonResource shadowSampler,
            Vector3 shadowPosition,
            Vector3 shadowPosDX, Vector3 shadowPosDY,
            uint cascadeIdx)
        {
            shadowPosition += shadowConstants.CascadeOffsets[cascadeIdx].XYZ();
            shadowPosition *= shadowConstants.CascadeScales[cascadeIdx].XYZ();

            shadowPosDX *= shadowConstants.CascadeScales[cascadeIdx].XYZ();
            shadowPosDY *= shadowConstants.CascadeScales[cascadeIdx].XYZ();

            var cascadeColor = Vector3.One;

            if (shadowConstants.VisualizeCascades == 1)
            {
                switch (cascadeIdx)
                {
                    case 0:
                        cascadeColor = new Vector3(1.0f, 0.0f, 0.0f);
                        break;

                    case 1:
                        cascadeColor = new Vector3(0.0f, 1.0f, 0.0f);
                        break;

                    case 2:
                        cascadeColor = new Vector3(0.0f, 0.0f, 1.0f);
                        break;

                    case 3:
                        cascadeColor = new Vector3(1.0f, 1.0f, 0.0f);
                        break;
                }
            }

            var shadow = SampleShadowMapOptimizedPcf(
                shadowConstants,
                shadowMap,
                shadowSampler,
                shadowPosition,
                shadowPosDX, shadowPosDY,
                cascadeIdx);

            return shadow * cascadeColor;
        }

        private static Vector3 GetShadowPosOffset(
            Global_ShadowConstantsPS shadowConstants,
            float nDotL,
            Vector3 normal)
        {
            var texelSize = 2.0f / shadowConstants.ShadowMapSize;
            var nmlOffsetScale = Saturate(1.0f - nDotL);
            return texelSize * shadowConstants.OffsetScale * nmlOffsetScale * normal;
        }

        public static Vector3 ShadowVisibility(
            Vector3 positionWS,
            float depthVS,
            float nDotL,
            Vector3 normal,
            Global_ShadowConstantsPS shadowConstants,
            DepthTexture2DArrayResource shadowMap,
            SamplerComparisonResource shadowSampler)
        {
            var shadowVisibility = Vector3.One;
            var cascadeIdx = 0u;

            // Figure out which cascade to sample from.
            for (uint i = 0; i < NumCascades - 1; ++i)
            {
                if (depthVS > shadowConstants.CascadeSplits[i])
                {
                    cascadeIdx = i + 1;
                }
            }

            // Apply offset
            var offset = GetShadowPosOffset(shadowConstants, nDotL, normal) / Abs(shadowConstants.CascadeScales[cascadeIdx].Z);

            // Project into shadow space
            var samplePos = positionWS + offset;
            var shadowPosition = Mul(shadowConstants.ShadowMatrix, new Vector4(samplePos, 1.0f)).XYZ();
            var shadowPosDX = DdxFine(shadowPosition);
            var shadowPosDY = DdyFine(shadowPosition);

            shadowVisibility = SampleShadowCascade(
                shadowConstants,
                shadowMap,
                shadowSampler,
                shadowPosition,
                shadowPosDX, shadowPosDY,
                cascadeIdx);

            if (shadowConstants.FilterAcrossCascades == 1)
            {
                // Sample the next cascade, and blend between the two results to
                // smooth the transition
                const float blendThreshold = 0.1f;
                var nextSplit = shadowConstants.CascadeSplits[cascadeIdx];
                var splitSize = cascadeIdx == 0 ? nextSplit : nextSplit - shadowConstants.CascadeSplits[cascadeIdx - 1];
                var splitDist = (nextSplit - depthVS) / splitSize;

                if (splitDist <= blendThreshold && cascadeIdx != NumCascades - 1)
                {
                    var nextSplitVisibility = SampleShadowCascade(
                        shadowConstants,
                        shadowMap,
                        shadowSampler,
                        shadowPosition,
                        shadowPosDX, shadowPosDY,
                        cascadeIdx + 1);
                    var lerpAmt = SmoothStep(0.0f, blendThreshold, splitDist);
                    shadowVisibility = Lerp(nextSplitVisibility, shadowVisibility, lerpAmt);
                }
            }

            return shadowVisibility;
        }
    }
}
