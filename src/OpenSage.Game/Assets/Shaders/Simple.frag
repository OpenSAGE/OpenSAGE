#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Lighting.h"
#include "Cloud.h"
#include "Shadows.h"
#include "Mesh.h"

MAKE_MESH_RESOURCES_PS()

layout(set = MESH_MATERIAL_RESOURCE_SET, binding = 0) uniform MaterialConstants
{
    vec4 ColorEmissive;
    vec4 TexCoordTransform_0;
} _MaterialConstants;

layout(set = MESH_MATERIAL_RESOURCE_SET, binding = 1) uniform texture2D Texture_0;

layout(location = 0) in vec3 in_WorldPosition;
layout(location = 1) in vec3 in_WorldNormal;
layout(location = 2) in vec2 in_UV0;
layout(location = 3) in vec2 in_CloudUV;

layout(location = 0) out vec4 out_Color;

void main()
{
    vec2 uv =
        (in_UV0 * _MaterialConstants.TexCoordTransform_0.xy) +
        (_GlobalConstants.TimeInSeconds * _MaterialConstants.TexCoordTransform_0.zw);

    vec4 color = vec4(_MaterialConstants.ColorEmissive.xyz, 1);

    color *= texture(sampler2D(Texture_0, Sampler), uv);

    vec3 cloudColor = GetCloudColor(Global_CloudTexture, Sampler, in_CloudUV);
    color = vec4(color.xyz * cloudColor, color.w);

    // TODO: fog

    out_Color = color;
}