#extension GL_EXT_samplerless_texture_functions : enable

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

float SampleShadowMap(
    texture2DArray shadowMap,
    samplerShadow shadowSampler,
    vec2 baseUv, float u, float v, vec2 shadowMapSizeInv,
    int cascadeIdx, float depth)
{
    vec2 uv = baseUv + vec2(u, v) * shadowMapSizeInv;
    float z = depth;

    return textureGrad(
        sampler2DArrayShadow(shadowMap, shadowSampler), 
        vec4(vec3(uv, cascadeIdx), z), 
        vec2(0, 0), 
        vec2(0, 0));
}

float SampleShadowMapOptimizedPCF(
    texture2DArray shadowMap,
    samplerShadow shadowSampler,
    vec3 shadowPos,
    vec3 shadowPosDX, vec3 shadowPosDY,
    int cascadeIdx, 
    ShadowConstantsPSType constants)
{
    vec3 shadowMapSize = textureSize(shadowMap, 0);

    float lightDepth = shadowPos.z;

    float bias = constants.Bias;

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
    switch (constants.ShadowsType)
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
            sampler2DArrayShadow(shadowMap, shadowSampler),
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

        sum += uw0 * vw0 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw0 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw0 * vw1 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw1 * SampleShadowMap(shadowMap, shadowSampler, baseUv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth);

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

vec3 SampleShadowCascade(
    texture2DArray shadowMap,
    samplerShadow shadowSampler,
    vec3 shadowPosition,
    vec3 shadowPosDX, vec3 shadowPosDY,
    int cascadeIdx, ivec2 screenPos,
    ShadowConstantsPSType constants)
{
    shadowPosition += constants.CascadeOffsets[cascadeIdx].xyz;
    shadowPosition *= constants.CascadeScales[cascadeIdx].xyz;

    shadowPosDX *= constants.CascadeScales[cascadeIdx].xyz;
    shadowPosDY *= constants.CascadeScales[cascadeIdx].xyz;

    vec3 cascadeColor = vec3(1.0f, 1.0f, 1.0f);

    if (constants.VisualizeCascades)
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
        shadowMap,
        shadowSampler,
        shadowPosition, 
        shadowPosDX, 
        shadowPosDY, 
        cascadeIdx, 
        constants);

    return shadow * cascadeColor;
}

vec3 GetShadowPosOffset(
    texture2DArray shadowMap, 
    float nDotL, 
    vec3 normal,
    ShadowConstantsPSType constants)
{
    vec3 shadowMapSize = textureSize(shadowMap, 0);

    float texelSize = 2.0f / shadowMapSize.x;
    float nmlOffsetScale = saturate(1.0f - nDotL);
    return texelSize * constants.OffsetScale * nmlOffsetScale * normal;
}

vec3 ShadowVisibility(
    texture2DArray shadowMap,
    samplerShadow shadowSampler,
    vec3 positionWS, float depthVS, float nDotL,
    vec3 normal, ivec2 screenPos,
    ShadowConstantsPSType constants)
{
    vec3 shadowVisibility = vec3(1, 1, 1);

    if (constants.ShadowsType == SHADOWS_NONE)
    {
        return shadowVisibility;
    }

    int cascadeIdx = 0;

    // Figure out which cascade to sample from.
    for (int i = 0; i < constants.NumSplits - 1; i++)
    {
        if (depthVS > constants.CascadeSplits[i])
        {
            cascadeIdx = i + 1;
        }
    }

    // Apply offset
    vec3 offset = GetShadowPosOffset(shadowMap, nDotL, normal, constants) / abs(constants.CascadeScales[cascadeIdx].z);

    // Project into shadow space
    vec3 samplePos = positionWS + offset;
    vec3 shadowPosition = (constants.ShadowMatrix * vec4(samplePos, 1.0f)).xyz;
    vec3 shadowPosDX = dFdxFine(shadowPosition);
    vec3 shadowPosDY = dFdyFine(shadowPosition);

    shadowVisibility = SampleShadowCascade(
        shadowMap,
        shadowSampler,
        shadowPosition,
        shadowPosDX, shadowPosDY, 
        cascadeIdx, screenPos,
        constants);

    if (constants.FilterAcrossCascades)
    {
        // Sample the next cascade, and blend between the two results to
        // smooth the transition
        const float blendThreshold = 0.1f;
        float nextSplit = constants.CascadeSplits[cascadeIdx];
        float splitSize = cascadeIdx == 0 ? nextSplit : nextSplit - constants.CascadeSplits[cascadeIdx - 1];
        float splitDist = (nextSplit - depthVS) / splitSize;

        if (splitDist <= blendThreshold && cascadeIdx != constants.NumSplits - 1)
        {
            vec3 nextSplitVisibility = SampleShadowCascade(
                shadowMap,
                shadowSampler,
                shadowPosition,
                shadowPosDX, shadowPosDY, 
                cascadeIdx + 1, screenPos,
                constants);
            float lerpAmt = smoothstep(0.0f, blendThreshold, splitDist);
            shadowVisibility = mix(nextSplitVisibility, shadowVisibility, lerpAmt);
        }
    }

    // Fade out shadows.
    float fade = smoothstep(constants.ShadowDistance * 0.9, constants.ShadowDistance, depthVS);
    shadowVisibility += vec3(fade, fade, fade);
    shadowVisibility = saturate(shadowVisibility);

    return shadowVisibility;
}