#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "ForwardPass.h"
#include "Mesh.h"

layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 0) uniform MaterialConstants
{
    float BumpScale;
    float SpecularExponent;
    bool AlphaTestEnable;

    float _Padding;

    vec4 AmbientColor;
    vec4 DiffuseColor;
    vec4 SpecularColor;
} _MaterialConstants;

layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 1) uniform texture2D DiffuseTexture;
layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 2) uniform texture2D NormalMap;
layout(set = MATERIAL_CONSTANTS_RESOURCE_SET, binding = 3) uniform sampler Sampler;

layout(location = 0) in vec3 in_WorldPosition;
layout(location = 1) in vec3 in_WorldNormal;
layout(location = 2) in vec2 in_UV0;
layout(location = 3) in vec2 in_CloudUV;
layout(location = 4) in vec3 in_WorldTangent;
layout(location = 5) in vec3 in_WorldBinormal;

layout(location = 0) out vec4 out_Color;

void main()
{
    vec2 uv = in_UV0;
    uv.y = 1 - uv.y;

    mat4 tangentToWorldSpace = mat4(
        in_WorldTangent.x, in_WorldTangent.y, in_WorldTangent.z, 0,
        in_WorldBinormal.x, in_WorldBinormal.y, in_WorldBinormal.z, 0,
        in_WorldNormal.x, in_WorldNormal.y, in_WorldNormal.z, 0,
        0, 0, 0, 1);

    vec3 tangentSpaceNormal = (texture(sampler2D(NormalMap, Sampler), uv).xyz * 2) - vec3(1, 1, 1);
    tangentSpaceNormal = vec3(tangentSpaceNormal.xy * _MaterialConstants.BumpScale, tangentSpaceNormal.z);
    tangentSpaceNormal = normalize(tangentSpaceNormal);

    vec3 worldSpaceNormal = TransformNormal(tangentSpaceNormal, tangentToWorldSpace);

    vec3 diffuseColor;
    vec3 specularColor;

    DoLighting(
        _GlobalLightingConstantsPS.Object,
        in_WorldPosition,
        worldSpaceNormal,
        _MaterialConstants.AmbientColor.xyz,
        _MaterialConstants.DiffuseColor.xyz,
        _MaterialConstants.SpecularColor.xyz,
        _MaterialConstants.SpecularExponent,
        false, // TODO: true
        vec3(1, 1, 1),
        diffuseColor,
        specularColor);

    vec4 diffuseTextureColor = texture(sampler2D(DiffuseTexture, Sampler), uv);

    if (_MaterialConstants.AlphaTestEnable)
    {
        if (FailsAlphaTest(diffuseTextureColor.w))
        {
            discard;
        }
    }

    vec3 objectColor = diffuseTextureColor.xyz * diffuseColor;

    objectColor += specularColor;

    vec3 cloudColor = GetCloudColor(in_CloudUV);
    objectColor *= cloudColor;

    out_Color = vec4(
        objectColor,
        _MaterialConstants.DiffuseColor.w * diffuseTextureColor.w);
}