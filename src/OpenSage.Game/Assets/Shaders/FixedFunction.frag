#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Lighting.h"
#include "Cloud.h"

layout(set = 0, binding = 0) uniform GlobalConstantsSharedUniform
{
    GlobalConstantsSharedType GlobalConstantsShared;
};

layout(set = 0, binding = 6) uniform GlobalConstantsPSUniform
{
    GlobalConstantsPSType GlobalConstantsPS;
};

layout(set = 0, binding = 7) uniform GlobalLightingConstantsPSBlock
{
    GlobalLightingConstantsPSType GlobalLightingConstantsPS;
};

layout(set = 0, binding = 8) uniform texture2D Global_CloudTexture;

#define TEXTURE_MAPPING_UV                 0
#define TEXTURE_MAPPING_ENVIRONMENT        1
#define TEXTURE_MAPPING_LINEAR_OFFSET      2
#define TEXTURE_MAPPING_ROTATE             3
#define TEXTURE_MAPPING_SINE_LINEAR_OFFSET 4
#define TEXTURE_MAPPING_SCREEN             5
#define TEXTURE_MAPPING_SCALE              6
#define TEXTURE_MAPPING_GRID               7

struct TextureMapping
{
    uint MappingType;

    float Speed;
    float Fps;
    uint Log2Width;

    vec2 UVPerSec;
    vec2 UVScale;
    vec2 UVCenter;
    vec2 UVAmplitude;
    vec2 UVFrequency;
    vec2 UVPhase;
};

struct VertexMaterial
{
    vec3 Ambient;

    float _Padding1;

    vec3 Diffuse;

    float _Padding2;

    vec3 Specular;
    float Shininess;
    vec3 Emissive;
    float Opacity;

    TextureMapping TextureMappingStage0;
    TextureMapping TextureMappingStage1;
};

#define DIFFUSE_LIGHTING_DISABLE  0
#define DIFFUSE_LIGHTING_MODULATE 1
#define DIFFUSE_LIGHTING_ADD      2

#define SECONDARY_TEXTURE_BLEND_DISABLE      0
#define SECONDARY_TEXTURE_BLEND_DETAIL       1
#define SECONDARY_TEXTURE_BLEND_SCALE        2
#define SECONDARY_TEXTURE_BLEND_INV_SCALE    3
#define SECONDARY_TEXTURE_BLEND_DETAIL_BLEND 4

struct ShadingConfiguration
{
    uint DiffuseLightingType;
    bool SpecularEnabled;
    bool TexturingEnabled;
    uint SecondaryTextureColorBlend;
    uint SecondaryTextureAlphaBlend;
    bool AlphaTest;

    vec2 _Padding;
};

struct MaterialConstantsType
{
    vec3 _Padding;

    uint NumTextureStages;

    VertexMaterial Material;
    ShadingConfiguration Shading;
};

layout(set = 0, binding = 9) uniform MaterialConstantsBlock
{
    MaterialConstantsType MaterialConstants;
};

layout(set = 0, binding = 10) uniform texture2D Texture0;
layout(set = 0, binding = 11) uniform texture2D Texture1;
layout(set = 0, binding = 12) uniform sampler Sampler;

layout(location = 0) in vec3 in_WorldPosition;
layout(location = 1) in vec3 in_WorldNormal;
layout(location = 2) in vec2 in_UV0;
layout(location = 3) in vec2 in_UV1;
layout(location = 4) in vec2 in_CloudUV;

in vec4 gl_FragCoord;

layout(location = 0) out vec4 out_Color;

vec4 SampleTexture(
    vec3 worldNormal, vec2 uv, vec2 screenPosition,
    TextureMapping textureMapping,
    texture2D diffuseTexture,
    vec3 viewVector)
{
    const float twoPi = 2 * 3.1415926535f;

    float t = GlobalConstantsShared.TimeInSeconds;

    switch (textureMapping.MappingType)
    {
        case TEXTURE_MAPPING_UV:
        {
            uv = vec2(uv.x, 1 - uv.y);
            break;
        }

        case TEXTURE_MAPPING_ENVIRONMENT:
        {
            uv = (reflect(viewVector, worldNormal).xy / 2.0) + vec2(0.5f, 0.5f);
            break;
        }

        case TEXTURE_MAPPING_LINEAR_OFFSET:
        {
            vec2 offset = textureMapping.UVPerSec * t;
            uv = vec2(uv.x, 1 - uv.y) + offset;
            uv *= textureMapping.UVScale;
            break;
        }

        case TEXTURE_MAPPING_ROTATE:
        {
            float angle = textureMapping.Speed * t * twoPi;
            float s = sin(angle);
            float c = cos(angle);

            uv -= textureMapping.UVCenter;

            vec2 rotatedPoint = vec2(
                uv.x * c - uv.y * s,
                uv.x * s + uv.y * c);

            uv = rotatedPoint + textureMapping.UVCenter;

            uv *= textureMapping.UVScale;

            break;
        }

        case TEXTURE_MAPPING_SINE_LINEAR_OFFSET:
        {
            uv.x += textureMapping.UVAmplitude.x * sin(textureMapping.UVFrequency.x * t * twoPi - textureMapping.UVPhase.x * twoPi);
            uv.y += textureMapping.UVAmplitude.y * cos(textureMapping.UVFrequency.y * t * twoPi - textureMapping.UVPhase.y * twoPi);
            break;
        }

        case TEXTURE_MAPPING_SCREEN:
        {
            uv = (screenPosition / GlobalConstantsPS.ViewportSize) * textureMapping.UVScale;
            break;
        }

        case TEXTURE_MAPPING_SCALE:
        {
            uv *= textureMapping.UVScale;
            break;
        }

        case TEXTURE_MAPPING_GRID:
        {
            uv = vec2(uv.x, 1 - uv.y);
            uint numFramesPerSide = uint(pow(2u, textureMapping.Log2Width));
            uint numFrames = numFramesPerSide * numFramesPerSide;
            uint currentFrame = uint(mod(t * textureMapping.Fps, numFrames));
            uint currentFrameU = uint(mod(currentFrame, numFramesPerSide));
            uint currentFrameV = currentFrame / numFramesPerSide;
            uv.x += currentFrameU / numFramesPerSide;
            uv.y += currentFrameV / numFramesPerSide;
            break;
        }
    }

    return texture(sampler2D(diffuseTexture, Sampler), uv);
}

void main()
{
    vec3 diffuseColor;
    vec3 specularColor;

    DoLighting(
        GlobalLightingConstantsPS,
        in_WorldPosition,
        in_WorldNormal,
        MaterialConstants.Material.Ambient,
        MaterialConstants.Material.Diffuse,
        MaterialConstants.Material.Specular,
        MaterialConstants.Material.Shininess,
        GlobalConstantsShared.CameraPosition,
        true,
        diffuseColor,
        specularColor);

    vec4 diffuseTextureColor;
    if (MaterialConstants.Shading.TexturingEnabled)
    {
        vec3 v = CalculateViewVector(GlobalConstantsShared.CameraPosition, in_WorldPosition);

        diffuseTextureColor = SampleTexture(
            in_WorldNormal, in_UV0, gl_FragCoord.xy,
            MaterialConstants.Material.TextureMappingStage0,
            Texture0,
            v);

        if (MaterialConstants.NumTextureStages > 1u)
        {
            vec4 secondaryTextureColor = SampleTexture(
                in_WorldNormal, in_UV1, gl_FragCoord.xy,
                MaterialConstants.Material.TextureMappingStage1,
                Texture1,
                v);

            switch (MaterialConstants.Shading.SecondaryTextureColorBlend)
            {
                case SECONDARY_TEXTURE_BLEND_DETAIL:
                    diffuseTextureColor = vec4(
                        secondaryTextureColor.xyz,
                        diffuseTextureColor.w);
                    break;

                case SECONDARY_TEXTURE_BLEND_SCALE:
                    diffuseTextureColor = vec4(
                        diffuseTextureColor.xyz * secondaryTextureColor.xyz,
                        diffuseTextureColor.w);
                    break;

                case SECONDARY_TEXTURE_BLEND_INV_SCALE:
                    diffuseTextureColor = vec4(
                        (vec3(1, 1, 1) - diffuseTextureColor.xyz) * secondaryTextureColor.xyz,
                        diffuseTextureColor.w);
                    break;

                case SECONDARY_TEXTURE_BLEND_DETAIL_BLEND:
                    // (otherAlpha)*local + (~otherAlpha)*other
                    diffuseTextureColor = vec4(
                        (secondaryTextureColor.x * diffuseTextureColor.xyz) + ((1 - secondaryTextureColor.x) * secondaryTextureColor.xyz),
                        diffuseTextureColor.w);
                    break;
            }

            switch (MaterialConstants.Shading.SecondaryTextureAlphaBlend)
            {
                case SECONDARY_TEXTURE_BLEND_DETAIL:
                    diffuseTextureColor.w = secondaryTextureColor.w;
                    break;

                case SECONDARY_TEXTURE_BLEND_SCALE:
                    diffuseTextureColor.w *= secondaryTextureColor.w;
                    break;

                case SECONDARY_TEXTURE_BLEND_INV_SCALE:
                    diffuseTextureColor.w += (1 - diffuseTextureColor.w) * secondaryTextureColor.w;
                    break;
            }
        }

        if (MaterialConstants.Shading.AlphaTest)
        {
            if (FailsAlphaTest(diffuseTextureColor.w))
            {
                discard;
            }
        }
    }
    else
    {
        diffuseTextureColor = vec4(1, 1, 1, 1);
    }
    
    vec3 totalObjectLighting = saturate(diffuseColor + MaterialConstants.Material.Emissive);

    vec3 objectColor = diffuseTextureColor.xyz;

    switch (MaterialConstants.Shading.DiffuseLightingType)
    {
        case DIFFUSE_LIGHTING_MODULATE:
            objectColor *= totalObjectLighting;
            break;

        case DIFFUSE_LIGHTING_ADD:
            objectColor += totalObjectLighting;
            break;
    }

    if (MaterialConstants.Shading.SpecularEnabled)
    {
        objectColor += specularColor;
    }

    vec3 cloudColor = GetCloudColor(
        Global_CloudTexture,
        Sampler,
        in_CloudUV);

    out_Color = vec4(
        objectColor * cloudColor,
        MaterialConstants.Material.Opacity * diffuseTextureColor.w);
}