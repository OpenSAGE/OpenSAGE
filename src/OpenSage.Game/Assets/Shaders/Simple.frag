#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Cloud.h"

layout(set = 0, binding = 0) uniform GlobalConstantsSharedUniform
{
    GlobalConstantsSharedType GlobalConstantsShared;
};

layout(set = 0, binding = 6) uniform texture2D Global_CloudTexture;

struct MaterialConstantsType
{
    vec4 ColorEmissive;
    vec4 TexCoordTransform_0;
};

layout(set = 0, binding = 7) uniform MaterialConstantsBlock
{
    MaterialConstantsType MaterialConstants;
};

layout(set = 0, binding = 8) uniform texture2D Texture_0;
layout(set = 0, binding = 9) uniform sampler Sampler;

layout(location = 0) in vec3 in_WorldPosition;
layout(location = 1) in vec3 in_WorldNormal;
layout(location = 2) in vec2 in_UV0;
layout(location = 3) in vec2 in_CloudUV;

layout(location = 0) out vec4 out_Color;

void main()
{
    vec2 uv =
        (in_UV0 * MaterialConstants.TexCoordTransform_0.xy) +
        (GlobalConstantsShared.TimeInSeconds * MaterialConstants.TexCoordTransform_0.zw);

    vec4 color = vec4(MaterialConstants.ColorEmissive.xyz, 1);

    color *= texture(sampler2D(Texture_0, Sampler), uv);

    vec3 cloudColor = GetCloudColor(Global_CloudTexture, Sampler, in_CloudUV);
    color = vec4(color.xyz * cloudColor, color.w);

    // TODO: fog

    out_Color = color;
}