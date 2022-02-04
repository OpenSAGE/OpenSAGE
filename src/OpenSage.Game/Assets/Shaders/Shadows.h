#ifndef SHADOWS_H

#define SHADOWS_H

#define NUM_CASCADES 4

#define SHADOWS_NONE 0
#define SHADOWS_HARD 1
#define SHADOWS_SOFT 2

struct ShadowConstantsPSType
{
    mat4 ShadowMatrix;
    vec4 CascadeSplits;
    vec4 CascadeOffsets[NUM_CASCADES];
    vec4 CascadeScales[NUM_CASCADES];
    float Bias;
    float OffsetScale;
    bool VisualizeCascades;
    bool FilterAcrossCascades;
    float ShadowDistance;
    int ShadowsType;
    int NumSplits;
    float _Padding;
};

layout(set = PASS_CONSTANTS_RESOURCE_SET, binding = 4) uniform ShadowConstantsPS
{
    ShadowConstantsPSType _ShadowConstantsPS;
};

layout(set = PASS_CONSTANTS_RESOURCE_SET, binding = 5) uniform texture2DArray Global_ShadowMap;

layout(set = PASS_CONSTANTS_RESOURCE_SET, binding = 6) uniform samplerShadow Global_ShadowSampler;

#ifndef COMPILING_FOR_VERTEX_SHADER

float SampleShadowMap(
    vec2 baseUv, float u, float v, vec2 shadowMapSizeInv,
    int cascadeIdx, float depth)
{
    vec2 uv = baseUv + vec2(u, v) * shadowMapSizeInv;
    float z = depth;

    return textureGrad(
        sampler2DArrayShadow(Global_ShadowMap, Global_ShadowSampler),
        vec4(vec3(uv, cascadeIdx), z), 
        vec2(0, 0), 
        vec2(0, 0));
}

float SampleShadowMapOptimizedPCF(
    vec3 shadowPos,
    vec3 shadowPosDX, vec3 shadowPosDY,
    int cascadeIdx)
{
    vec3 shadowMapSize = textureSize(sampler2DArrayShadow(Global_ShadowMap, Global_ShadowSampler), 0);

    float lightDepth = shadowPos.z;

    float bias = _ShadowConstantsPS.Bias;

    lightDepth -= bias;

    vec2 uv = shadowPos.xy * shadowMapSize.xy; // 1 unit - 1 texel

    vec2 shadowMapSizeInv = 1.0 / shadowMapSize.xy;

    vec2 baseUv;
    baseUv.x = floor(uv.x + 0.5);
    baseUv.y = floor(uv.y + 0.5);

    float s = (uv.x + 0.5 - baseUv.x);
    float t = (uv.y + 0.5 - baseUv.y);

    baseUv -= vec2(0.5, 0.5);
    baseUv *= shadowMapSizeInv;

    float sum = 0;

    int filterSize;
    switch (_ShadowConstantsPS.ShadowsType)
    {
    case SHADOWS_SOFT:
        filterSize = 5;
        break;

    default:
        filterSize = 2;
        break;
    }

    if (filterSize == 2)
    {
        return textureGrad(
            sampler2DArrayShadow(Global_ShadowMap, Global_ShadowSampler),
            vec4(vec3(shadowPos.xy, cascadeIdx), lightDepth),
            vec2(0, 0),
            vec2(0, 0));
    }
    else if (filterSize == 3)
    {
        float uw0 = (3 - 2 * s);
        float uw1 = (1 + 2 * s);

        float u0 = (2 - s) / uw0 - 1;
        float u1 = s / uw1 + 1;

        float vw0 = (3 - 2 * t);
        float vw1 = (1 + 2 * t);

        float v0 = (2 - t) / vw0 - 1;
        float v1 = t / vw1 + 1;

        sum += uw0 * vw0 * SampleShadowMap(baseUv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw0 * SampleShadowMap(baseUv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw0 * vw1 * SampleShadowMap(baseUv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw1 * SampleShadowMap(baseUv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth);

        return sum * 1.0f / 16;
    }
    else if (filterSize == 5)
    {
        float uw0 = (4 - 3 * s);
        float uw1 = 7;
        float uw2 = (1 + 3 * s);

        float u0 = (3 - 2 * s) / uw0 - 2;
        float u1 = (3 + s) / uw1;
        float u2 = s / uw2 + 2;

        float vw0 = (4 - 3 * t);
        float vw1 = 7;
        float vw2 = (1 + 3 * t);

        float v0 = (3 - 2 * t) / vw0 - 2;
        float v1 = (3 + t) / vw1;
        float v2 = t / vw2 + 2;

        sum += uw0 * vw0 * SampleShadowMap(baseUv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw0 * SampleShadowMap(baseUv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw0 * SampleShadowMap(baseUv, u2, v0, shadowMapSizeInv, cascadeIdx, lightDepth);

        sum += uw0 * vw1 * SampleShadowMap(baseUv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw1 * SampleShadowMap(baseUv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw1 * SampleShadowMap(baseUv, u2, v1, shadowMapSizeInv, cascadeIdx, lightDepth);

        sum += uw0 * vw2 * SampleShadowMap(baseUv, u0, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw2 * SampleShadowMap(baseUv, u1, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw2 * SampleShadowMap(baseUv, u2, v2, shadowMapSizeInv, cascadeIdx, lightDepth);

        return sum * 1.0f / 144;
    }
    else // filterSize == 7
    {
        float uw0 = (5 * s - 6);
        float uw1 = (11 * s - 28);
        float uw2 = -(11 * s + 17);
        float uw3 = -(5 * s + 1);

        float u0 = (4 * s - 5) / uw0 - 3;
        float u1 = (4 * s - 16) / uw1 - 1;
        float u2 = -(7 * s + 5) / uw2 + 1;
        float u3 = -s / uw3 + 3;

        float vw0 = (5 * t - 6);
        float vw1 = (11 * t - 28);
        float vw2 = -(11 * t + 17);
        float vw3 = -(5 * t + 1);

        float v0 = (4 * t - 5) / vw0 - 3;
        float v1 = (4 * t - 16) / vw1 - 1;
        float v2 = -(7 * t + 5) / vw2 + 1;
        float v3 = -t / vw3 + 3;

        sum += uw0 * vw0 * SampleShadowMap(baseUv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw0 * SampleShadowMap(baseUv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw0 * SampleShadowMap(baseUv, u2, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw3 * vw0 * SampleShadowMap(baseUv, u3, v0, shadowMapSizeInv, cascadeIdx, lightDepth);

        sum += uw0 * vw1 * SampleShadowMap(baseUv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw1 * SampleShadowMap(baseUv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw1 * SampleShadowMap(baseUv, u2, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw3 * vw1 * SampleShadowMap(baseUv, u3, v1, shadowMapSizeInv, cascadeIdx, lightDepth);

        sum += uw0 * vw2 * SampleShadowMap(baseUv, u0, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw2 * SampleShadowMap(baseUv, u1, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw2 * SampleShadowMap(baseUv, u2, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw3 * vw2 * SampleShadowMap(baseUv, u3, v2, shadowMapSizeInv, cascadeIdx, lightDepth);

        sum += uw0 * vw3 * SampleShadowMap(baseUv, u0, v3, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw3 * SampleShadowMap(baseUv, u1, v3, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw3 * SampleShadowMap(baseUv, u2, v3, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw3 * vw3 * SampleShadowMap(baseUv, u3, v3, shadowMapSizeInv, cascadeIdx, lightDepth);

        return sum * 1.0f / 2704;
    }
}

vec3 SampleShadowCascade(
    vec3 shadowPosition,
    vec3 shadowPosDX, vec3 shadowPosDY,
    int cascadeIdx, ivec2 screenPos)
{
    shadowPosition += _ShadowConstantsPS.CascadeOffsets[cascadeIdx].xyz;
    shadowPosition *= _ShadowConstantsPS.CascadeScales[cascadeIdx].xyz;

    shadowPosDX *= _ShadowConstantsPS.CascadeScales[cascadeIdx].xyz;
    shadowPosDY *= _ShadowConstantsPS.CascadeScales[cascadeIdx].xyz;

    vec3 cascadeColor = vec3(1.0f, 1.0f, 1.0f);

    if (_ShadowConstantsPS.VisualizeCascades)
    {
        const vec3 cascadeColors[NUM_CASCADES] =
        {
            vec3(1.0f, 0.0f, 0.0f),
            vec3(0.0f, 1.0f, 0.0f),
            vec3(0.0f, 0.0f, 1.0f),
            vec3(1.0f, 1.0f, 0.0f)
        };

        cascadeColor = cascadeColors[cascadeIdx];
    }

    float shadow = SampleShadowMapOptimizedPCF(
        shadowPosition, 
        shadowPosDX, 
        shadowPosDY, 
        cascadeIdx);

    return shadow * cascadeColor;
}

vec3 GetShadowPosOffset(
    float nDotL, 
    vec3 normal)
{
    vec3 shadowMapSize = textureSize(sampler2DArrayShadow(Global_ShadowMap, Global_ShadowSampler), 0);

    float texelSize = 2.0f / shadowMapSize.x;
    float nmlOffsetScale = saturate(1.0f - nDotL);
    return texelSize * _ShadowConstantsPS.OffsetScale * nmlOffsetScale * normal;
}

vec3 ShadowVisibility(
    vec3 positionWS, float depthVS, float nDotL,
    vec3 normal, ivec2 screenPos)
{
    vec3 shadowVisibility = vec3(1, 1, 1);

    if (_ShadowConstantsPS.ShadowsType == SHADOWS_NONE)
    {
        return shadowVisibility;
    }

    int cascadeIdx = 0;

    // Figure out which cascade to sample from.
    for (int i = 0; i < _ShadowConstantsPS.NumSplits - 1; i++)
    {
        if (depthVS > _ShadowConstantsPS.CascadeSplits[i])
        {
            cascadeIdx = i + 1;
        }
    }

    // Apply offset
    vec3 offset = GetShadowPosOffset(nDotL, normal) / abs(_ShadowConstantsPS.CascadeScales[cascadeIdx].z);

    // Project into shadow space
    vec3 samplePos = positionWS + offset;
    vec3 shadowPosition = (_ShadowConstantsPS.ShadowMatrix * vec4(samplePos, 1.0f)).xyz;
    vec3 shadowPosDX = dFdxFine(shadowPosition);
    vec3 shadowPosDY = dFdyFine(shadowPosition);

    shadowVisibility = SampleShadowCascade(
        shadowPosition,
        shadowPosDX, shadowPosDY, 
        cascadeIdx, screenPos);

    if (_ShadowConstantsPS.FilterAcrossCascades)
    {
        // Sample the next cascade, and blend between the two results to
        // smooth the transition
        const float blendThreshold = 0.1f;
        float nextSplit = _ShadowConstantsPS.CascadeSplits[cascadeIdx];
        float splitSize = cascadeIdx == 0 ? nextSplit : nextSplit - _ShadowConstantsPS.CascadeSplits[cascadeIdx - 1];
        float splitDist = (nextSplit - depthVS) / splitSize;

        if (splitDist <= blendThreshold && cascadeIdx != _ShadowConstantsPS.NumSplits - 1)
        {
            vec3 nextSplitVisibility = SampleShadowCascade(
                shadowPosition,
                shadowPosDX, shadowPosDY, 
                cascadeIdx + 1, screenPos);
            float lerpAmt = smoothstep(0.0f, blendThreshold, splitDist);
            shadowVisibility = mix(nextSplitVisibility, shadowVisibility, lerpAmt);
        }
    }

    // Fade out shadows.
    float fade = smoothstep(_ShadowConstantsPS.ShadowDistance * 0.9, _ShadowConstantsPS.ShadowDistance, depthVS);
    shadowVisibility += vec3(fade, fade, fade);
    shadowVisibility = saturate(shadowVisibility);

    return shadowVisibility;
}

#endif

#endif