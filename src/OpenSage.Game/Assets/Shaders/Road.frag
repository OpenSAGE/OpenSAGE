#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Lighting.h"
#include "Cloud.h"
#include "Shadows.h"

layout(set = 0, binding = 0) uniform GlobalConstantsShared
{
    GlobalConstantsSharedType _GlobalConstantsShared;
};

layout(set = 0, binding = 3) uniform GlobalLightingConstantsPS
{
    GlobalLightingConstantsPSType _GlobalLightingConstantsPS;
};

layout(set = 0, binding = 4) uniform texture2D Global_CloudTexture;

layout(set = 0, binding = 5) uniform texture2D Texture;
layout(set = 0, binding = 6) uniform sampler Sampler;

layout(set = 0, binding = 7) uniform ShadowConstantsPS
{
    ShadowConstantsPSType _ShadowConstantsPS;
};

layout(set = 0, binding = 8) uniform texture2DArray Global_ShadowMap;
layout(set = 0, binding = 9) uniform samplerShadow Global_ShadowSampler;

layout(location = 0) in vec3 in_WorldPosition;
layout(location = 1) in vec3 in_WorldNormal;
layout(location = 2) in vec2 in_UV;
layout(location = 3) in vec2 in_CloudUV;
layout(location = 4) in float in_ViewSpaceDepth;

in vec4 gl_FragCoord;

layout(location = 0) out vec4 out_Color;

void main()
{
    float nDotL = saturate(dot(in_WorldNormal, -_GlobalLightingConstantsPS.Lights[0].Direction));
    vec3 shadowVisibility = ShadowVisibility(
        Global_ShadowMap,
        Global_ShadowSampler,
        in_WorldPosition, 
        in_ViewSpaceDepth, 
        nDotL, 
        in_WorldNormal, 
        ivec2(gl_FragCoord.xy), 
        _ShadowConstantsPS);

    vec3 diffuseColor;
    vec3 specularColor;

    DoLighting(
        _GlobalLightingConstantsPS,
        in_WorldPosition,
        in_WorldNormal,
        vec3(1, 1, 1),
        vec3(1, 1, 1),
        vec3(0, 0, 0),
        0,
        _GlobalConstantsShared.CameraPosition,
        false,
        shadowVisibility,
        diffuseColor,
        specularColor);

    vec4 textureColor = texture(sampler2D(Texture, Sampler), in_UV);

    vec3 cloudColor = GetCloudColor(Global_CloudTexture, Sampler, in_CloudUV);

    out_Color = vec4(
        diffuseColor * textureColor.xyz * cloudColor,
        textureColor.w);
}