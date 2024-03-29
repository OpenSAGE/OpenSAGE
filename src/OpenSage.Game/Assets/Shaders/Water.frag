#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "ForwardPass.h"

layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 0) uniform WaterConstantsPS
{
    vec2 UVOffset;
    float FarPlaneDistance;
    float NearPlaneDistance;
    uint IsRenderReflection;
    uint IsRenderRefraction;
    float TransparentWaterMinOpacity;
    float TransparentWaterDepth;
    vec4 DiffuseColor;
    vec4 TransparentDiffuseColor;
};

layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 1) uniform texture2D WaterTexture;
layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 2) uniform texture2D BumpTexture;
layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 3) uniform sampler WaterSampler;
layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 4) uniform texture2D ReflectionMap;
layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 5) uniform sampler ReflectionMapSampler;
layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 6) uniform texture2D RefractionMap;
layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 7) uniform sampler RefractionMapSampler;
layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 8) uniform texture2D RefractionDepthMap;

layout(location = 0) in vec3 in_WorldPosition;
layout(location = 1) in vec2 in_CloudUV;
layout(location = 2) in float in_ViewSpaceDepth;

in vec4 gl_FragCoord;

layout(location = 0) out vec4 out_Color;

const float distortionPower = 0.05f;
const float depthFactor = 2.0f;
const float minWaterDepth = 20.0f;

float getLinearDepthMap(float nearPlaneDistance, float farPlaneDistance, float depth)
{
    float result = 2.0f * nearPlaneDistance * farPlaneDistance / (farPlaneDistance + nearPlaneDistance - (2.0f * depth - 1.0f) * (farPlaneDistance - nearPlaneDistance));
    return result;
}

void main()
{
    vec2 waterUV = vec2(in_WorldPosition.x / 320, in_WorldPosition.y / 320);
    vec3 viewVector = CalculateViewVector(in_WorldPosition);
        
    //this assumes that the normal vector points upwards
    float fresnelFactor = dot(viewVector, vec3(0.0f, 0.0f, 1.0f));

    vec2 normalDeviceCoord = vec2(gl_FragCoord.x / _GlobalConstants.ViewportSize.x, gl_FragCoord.y / _GlobalConstants.ViewportSize.y);
    vec2 distortion = (texture(sampler2D(WaterTexture, WaterSampler), waterUV - UVOffset).xy * 2.0f - 1.0f) * distortionPower;
    vec2 reflectionMapUV = vec2(clamp(normalDeviceCoord.x, 0.001f, 0.999f), clamp(normalDeviceCoord.y, 0.001f, 0.999)); // Add minus to y coord if not using stencil clipping
    vec2 refractionMapUV = vec2(clamp(normalDeviceCoord.x, 0.001f, 0.999f), clamp(normalDeviceCoord.y, 0.001f, 0.999));
    
    vec4 textureColor = texture(sampler2D(WaterTexture, WaterSampler), waterUV + distortion);
    vec4 reflectionColor = texture(sampler2D(ReflectionMap, ReflectionMapSampler), reflectionMapUV + distortion);
    vec4 refractionColor = texture(sampler2D(RefractionMap, RefractionMapSampler), refractionMapUV + distortion);
    float refractionDepth = texture(sampler2D(RefractionDepthMap, RefractionMapSampler), refractionMapUV).x;

    float linearRefractionDepth = getLinearDepthMap(NearPlaneDistance, FarPlaneDistance, refractionDepth);
    float linearPlaneDepth = getLinearDepthMap(NearPlaneDistance, FarPlaneDistance, gl_FragCoord.z);
    float linearWaterDepth = linearRefractionDepth -  linearPlaneDepth;
    
    vec3 cloudColor = GetCloudColor(in_CloudUV + distortion);
    vec3 worldNormal = texture(sampler2D(BumpTexture, WaterSampler), waterUV + distortion).xyz;

    float nDotL = saturate(dot(worldNormal, -_GlobalLightingConstantsPS.Terrain.Lights[0].Direction));
    vec3 shadowVisibility = ShadowVisibility(
        in_WorldPosition, 
        in_ViewSpaceDepth, 
        nDotL, 
        worldNormal, 
        ivec2(gl_FragCoord.xy));

    vec3 diffuseColor;
    vec3 specularColor;

    DoLighting(
        _GlobalLightingConstantsPS.Terrain,
        in_WorldPosition,
        worldNormal,
        vec3(1, 1, 1),
        DiffuseColor.xyz,
        vec3(0, 0, 0),
        0,
        false,
        shadowVisibility,
        diffuseColor,
        specularColor);

    // Calculate water transparency
    float alpha = clamp((linearWaterDepth/depthFactor)/TransparentWaterDepth, 0.0f, TransparentWaterMinOpacity);
    vec4 finalColor = vec4(diffuseColor * textureColor.xyz * cloudColor, alpha);

    if (IsRenderReflection != 0 && IsRenderRefraction != 0)
    {
        float refractionFactor = linearWaterDepth - minWaterDepth;
        vec4 fresnelColor = mix(reflectionColor, refractionColor, fresnelFactor);
        finalColor.a *= fresnelColor.w;
        finalColor.xyz *= fresnelColor.xyz;
    }
    else if (IsRenderReflection != 0)
    {
        finalColor.xyz *= reflectionColor.xyz;
    }
    else if (IsRenderRefraction != 0) 
    {
        finalColor.xyz *= refractionColor.xyz;
    }
    out_Color = finalColor;
}