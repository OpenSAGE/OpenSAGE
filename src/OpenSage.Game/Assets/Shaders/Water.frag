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

layout(set = 4, binding = 0) uniform texture2D WaterTexture;
layout(set = 4, binding = 1) uniform sampler Sampler;

layout(location = 0) in vec3 in_WorldPosition;
layout(location = 1) in vec2 in_CloudUV;
layout(location = 2) in float in_ViewSpaceDepth;

in vec4 gl_FragCoord;

layout(location = 0) out vec4 out_Color;

void main()
{
    vec3 worldNormal = vec3(0, 0, 1);

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

    vec2 waterUV = vec2(in_WorldPosition.x / 320, in_WorldPosition.y / 320);

    vec4 textureColor = texture(sampler2D(WaterTexture, Sampler), waterUV);

    vec3 cloudColor = GetCloudColor(Global_CloudTexture, Sampler, in_CloudUV);

    out_Color = vec4(
        diffuseColor * textureColor.xyz * cloudColor,
        textureColor.w);
}