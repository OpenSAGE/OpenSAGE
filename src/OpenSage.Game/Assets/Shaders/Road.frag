#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "ForwardPass.h"

layout(set = 2, binding = 0) uniform texture2D Texture;
layout(set = 2, binding = 1) uniform sampler Sampler;

layout(location = 0) in vec3 in_WorldPosition;
layout(location = 1) in vec3 in_WorldNormal;
layout(location = 2) in vec2 in_UV;
layout(location = 3) in vec2 in_CloudUV;
layout(location = 4) in float in_ViewSpaceDepth;

in vec4 gl_FragCoord;

layout(location = 0) out vec4 out_Color;

void main()
{
    float nDotL = saturate(dot(in_WorldNormal, -_GlobalLightingConstantsPS.Terrain.Lights[0].Direction));
    vec3 shadowVisibility = ShadowVisibility(
        in_WorldPosition, 
        in_ViewSpaceDepth, 
        nDotL, 
        in_WorldNormal, 
        ivec2(gl_FragCoord.xy));

    vec3 diffuseColor;
    vec3 specularColor;

    DoLighting(
        _GlobalLightingConstantsPS.Terrain,
        in_WorldPosition,
        in_WorldNormal,
        vec3(1, 1, 1),
        vec3(1, 1, 1),
        vec3(0, 0, 0),
        0,
        false,
        shadowVisibility,
        diffuseColor,
        specularColor);

    vec4 textureColor = texture(sampler2D(Texture, Sampler), in_UV);

    vec3 cloudColor = GetCloudColor(in_CloudUV);

    vec3 decalColor = GetRadiusCursorDecalColor(in_WorldPosition);

    out_Color = vec4(
        (diffuseColor * textureColor.xyz * cloudColor) + decalColor,
        textureColor.w);
}