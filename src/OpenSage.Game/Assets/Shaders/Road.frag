#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Lighting.h"
#include "Cloud.h"

layout(set = 0, binding = 0) uniform GlobalConstantsSharedUniform
{
    GlobalConstantsSharedType GlobalConstantsShared;
};

layout(set = 0, binding = 3) uniform GlobalLightingConstantsPSBlock
{
    GlobalLightingConstantsPSType GlobalLightingConstantsPS;
};

layout(set = 0, binding = 4) uniform texture2D Global_CloudTexture;

layout(set = 0, binding = 5) uniform texture2D Texture;
layout(set = 0, binding = 6) uniform sampler Sampler;

layout(location = 0) in vec3 in_WorldPosition;
layout(location = 1) in vec3 in_WorldNormal;
layout(location = 2) in vec2 in_UV;
layout(location = 3) in vec2 in_CloudUV;

layout(location = 0) out vec4 out_Color;

void main()
{
    vec3 diffuseColor;
    vec3 specularColor;

    DoLighting(
        GlobalLightingConstantsPS,
        in_WorldPosition,
        in_WorldNormal,
        vec3(1, 1, 1),
        vec3(1, 1, 1),
        vec3(0, 0, 0),
        0,
        GlobalConstantsShared.CameraPosition,
        false,
        diffuseColor,
        specularColor);

    vec4 textureColor = texture(sampler2D(Texture, Sampler), in_UV);

    vec3 cloudColor = GetCloudColor(Global_CloudTexture, Sampler, in_CloudUV);

    out_Color = vec4(
        diffuseColor * textureColor.xyz * cloudColor,
        textureColor.w);
}