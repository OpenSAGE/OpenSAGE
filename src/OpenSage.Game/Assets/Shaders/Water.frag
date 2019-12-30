#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Lighting.h"
#include "Cloud.h"
#include "Shadows.h"

MAKE_GLOBAL_CONSTANTS_RESOURCES_PS(0)

MAKE_GLOBAL_LIGHTING_CONSTANTS_RESOURCES_PS(1)

MAKE_GLOBAL_CLOUD_RESOURCES_PS(2)

MAKE_GLOBAL_SHADOW_RESOURCES_PS(3)

layout(set = 4, binding = 1) uniform WaterConstantsPS
{
	vec2 UVFactor;
    vec2 UVOffset;
    float FarPlaneDistance;
    float NearPlaneDistance;
    uint IsRenderReflection;
    uint IsRenderRefraction;
};
layout(set = 4, binding = 2) uniform texture2D WaterTexture;
layout(set = 4, binding = 3) uniform texture2D BumpTexture;
layout(set = 4, binding = 4) uniform sampler WaterSampler;
layout(set = 4, binding = 5) uniform texture2D ReflectionMap;
layout(set = 4, binding = 6) uniform sampler ReflectionMapSampler;
layout(set = 4, binding = 7) uniform texture2D RefractionMap;
layout(set = 4, binding = 8) uniform sampler RefractionMapSampler;
layout(set = 4, binding = 9) uniform texture2D RefractionDepthMap;

layout(location = 0) in vec3 in_WorldPosition;
layout(location = 1) in vec2 in_CloudUV;
layout(location = 2) in float in_ViewSpaceDepth;
layout(location = 3) in vec4 in_ClipSpace;
layout(location = 4) in vec3 in_ViewVector;

in vec4 gl_FragCoord;

layout(location = 0) out vec4 out_Color;

const float distortionPower = 0.05f;
const float depthFactor = 7.0f;
const float minWaterDepth = 20.0f;

float getLinearDepthMap(float nearPlaneDistance, float farPlaneDistance, float depth)
{
    float result = 2.0f * nearPlaneDistance * farPlaneDistance / (farPlaneDistance + nearPlaneDistance - (2.0f * depth - 1.0f) * (farPlaneDistance - nearPlaneDistance));
    return result;
}

void main()
{
    vec2 waterUV = vec2(in_WorldPosition.x / 320, in_WorldPosition.y / 320);
        
    float fresnelFactor = dot(in_ViewVector, vec3(0.0f, 0.0f, 1.0f));
    fresnelFactor = pow(fresnelFactor, 0.5f);

    vec2 normalDeviceCoord = vec2(gl_FragCoord.x / _GlobalConstantsPS.ViewportSize.x, gl_FragCoord.y / _GlobalConstantsPS.ViewportSize.y);
    vec2 distortion = (texture(sampler2D(WaterTexture, WaterSampler), waterUV - UVOffset).xy * 2.0f - 1.0f) * distortionPower;
    vec2 reflectionMapUV = vec2(clamp(normalDeviceCoord.x, 0.01f, 0.99f), clamp(normalDeviceCoord.y, 0.01f, 0.99f)); // Add minus to y coord if not using stencil clipping
    vec2 refractionMapUV = vec2(clamp(normalDeviceCoord.x, 0.01f, 0.99f), clamp(normalDeviceCoord.y, 0.01f, 0.99f));
    
    vec4 textureColor = texture(sampler2D(WaterTexture, WaterSampler), waterUV + distortion);
    vec4 reflectionColor = texture(sampler2D(ReflectionMap, ReflectionMapSampler), reflectionMapUV + distortion);
    vec4 refractionColor = texture(sampler2D(RefractionMap, RefractionMapSampler), refractionMapUV + distortion);
    vec4 refractionDepth = texture(sampler2D(RefractionDepthMap, RefractionMapSampler), refractionMapUV);

    float linearRefractionDepth = getLinearDepthMap(NearPlaneDistance, FarPlaneDistance, refractionDepth.x);
    float linearPlaneDepth = getLinearDepthMap(NearPlaneDistance, FarPlaneDistance, gl_FragCoord.z);
    float linearWaterDepth = linearRefractionDepth -  linearPlaneDepth;
    float waterDepth = clamp((linearWaterDepth)/depthFactor, 0.0f, 1.0f);
    
    vec3 cloudColor = GetCloudColor(Global_CloudTexture, WaterSampler, in_CloudUV + distortion);
    vec3 worldNormal = texture(sampler2D(BumpTexture, WaterSampler), waterUV + distortion).xyz;

    float nDotL = saturate(dot(worldNormal, -_GlobalLightingConstantsPS.Lights[0].Direction));
    vec3 shadowVisibility = ShadowVisibility(
        Global_ShadowMap,
        Global_ShadowSampler,
        in_WorldPosition, 
        in_ViewSpaceDepth, 
        nDotL, 
        worldNormal, 
        ivec2(gl_FragCoord.xy), 
        _ShadowConstantsPS);

    vec3 diffuseColor;
    vec3 specularColor;

    DoLighting(
        _GlobalLightingConstantsPS,
        in_WorldPosition,
        worldNormal,
        vec3(1, 1, 1),
        vec3(1, 1, 1),
        vec3(0, 0, 0),
        0,
        _GlobalConstantsShared.CameraPosition,
        false,
        shadowVisibility,
        diffuseColor,
        specularColor);

    vec4 finalColor;
    if (IsRenderReflection == 0)
    {
        finalColor = vec4(diffuseColor * refractionColor.xyz * textureColor.xyz * cloudColor, waterDepth);
    }
    else if (IsRenderRefraction == 0) {
        finalColor = vec4(diffuseColor * reflectionColor.xyz * textureColor.xyz * cloudColor, waterDepth);
    }
    else
    {
        float refractionFactor = linearWaterDepth - minWaterDepth;
        vec4 fresnelColor = mix(refractionColor, reflectionColor, fresnelFactor);
        float alpha = fresnelColor.w * waterDepth;
        finalColor = vec4(diffuseColor * fresnelColor.xyz * textureColor.xyz * cloudColor, alpha);
    }
    out_Color = finalColor;
}