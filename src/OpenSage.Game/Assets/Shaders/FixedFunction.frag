#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Lighting.h"
#include "Cloud.h"
#include "Shadows.h"
#include "Mesh.h"

layout(set = 0, binding = 0) uniform GlobalConstantsShared
{
    GlobalConstantsSharedType _GlobalConstantsShared;
};

layout(set = 0, binding = 3) uniform MeshConstants
{
    MeshConstantsType _MeshConstants;
};

layout(set = 0, binding = 6) uniform GlobalConstantsPS
{
    GlobalConstantsPSType _GlobalConstantsPS;
};

layout(set = 0, binding = 7) uniform GlobalLightingConstantsPS
{
    GlobalLightingConstantsPSType _GlobalLightingConstantsPS;
};

layout(set = 0, binding = 8) uniform texture2D Global_CloudTexture;

layout(set = 0, binding = 9) uniform RenderItemConstantsPS
{
    RenderItemConstantsPSType _RenderItemConstantsPS;
};

#define TEXTURE_MAPPING_UV                 0
#define TEXTURE_MAPPING_ENVIRONMENT        1
#define TEXTURE_MAPPING_LINEAR_OFFSET      2
#define TEXTURE_MAPPING_ROTATE             3
#define TEXTURE_MAPPING_SINE_LINEAR_OFFSET 4
#define TEXTURE_MAPPING_STEP_LINEAR_OFFSET 5
#define TEXTURE_MAPPING_SCREEN             6
#define TEXTURE_MAPPING_SCALE              7
#define TEXTURE_MAPPING_GRID               8
#define TEXTURE_MAPPING_RANDOM             9
#define TEXTURE_MAPPING_BUMP_ENV           10
#define TEXTURE_MAPPING_WS_ENVIRONMENT     11

struct TextureMapping
{
    int MappingType;

    float Speed;
    float Fps;
    int Log2Width;

    vec2 UVPerSec;
    vec2 UVScale;
    vec2 UVCenter;
    vec2 UVAmplitude;
    vec2 UVFrequency;
    vec2 UVPhase;
    
    vec2 UVStep;
    float StepsPerSecond;

    float _Padding;
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

#define DIFFUSE_LIGHTING_DISABLE             0
#define DIFFUSE_LIGHTING_MODULATE            1
#define DIFFUSE_LIGHTING_ADD                 2
#define DIFFUSE_LIGHTING_BUMP_ENV_MAP        3

#define SECONDARY_TEXTURE_BLEND_DISABLE      0
#define SECONDARY_TEXTURE_BLEND_DETAIL       1
#define SECONDARY_TEXTURE_BLEND_SCALE        2
#define SECONDARY_TEXTURE_BLEND_INV_SCALE    3
#define SECONDARY_TEXTURE_BLEND_DETAIL_BLEND 4
#define SECONDARY_TEXTURE_BLEND_ADD          5

struct ShadingConfiguration
{
    int DiffuseLightingType;
    bool SpecularEnabled;
    bool TexturingEnabled;
    int SecondaryTextureColorBlend;
    int SecondaryTextureAlphaBlend;
    bool AlphaTest;

    vec2 _Padding;
};

layout(set = 0, binding = 10) uniform MaterialConstants
{
    vec3 _Padding;

    int NumTextureStages;

    VertexMaterial Material;
    ShadingConfiguration Shading;
} _MaterialConstants;

layout(set = 0, binding = 11) uniform texture2D Texture0;
layout(set = 0, binding = 12) uniform texture2D Texture1;
layout(set = 0, binding = 13) uniform sampler Sampler;

layout(set = 0, binding = 14) uniform ShadowConstantsPS
{
    ShadowConstantsPSType _ShadowConstantsPS;
};

layout(set = 0, binding = 15) uniform texture2DArray Global_ShadowMap;
layout(set = 0, binding = 16) uniform samplerShadow Global_ShadowSampler;

layout(location = 0) in vec3 in_WorldPosition;
layout(location = 1) in vec3 in_WorldNormal;
layout(location = 2) in vec2 in_UV0;
layout(location = 3) in vec2 in_UV1;
layout(location = 4) in vec2 in_CloudUV;
layout(location = 5) in float in_ViewSpaceDepth;

in vec4 gl_FragCoord;

layout(location = 0) out vec4 out_Color;

vec4 SampleTexture(
    vec3 worldNormal, vec2 uv, vec2 screenPosition,
    TextureMapping textureMapping,
    texture2D diffuseTexture,
    vec3 viewVector)
{
    const float twoPi = 2 * 3.1415926535f;

    float t = _GlobalConstantsShared.TimeInSeconds;

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
            uv = vec2(uv.x, 1 - uv.y) - offset;
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

        case TEXTURE_MAPPING_STEP_LINEAR_OFFSET:
        {
            float steps = textureMapping.StepsPerSecond * t;
            int integralSteps = int(floor(steps));
            vec2 offset = textureMapping.UVStep * integralSteps;
            uv = vec2(uv.x, 1 - uv.y) + offset;
            break;
        }

        case TEXTURE_MAPPING_SCREEN:
        {
            uv = (screenPosition / _GlobalConstantsPS.ViewportSize) * textureMapping.UVScale;
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
            int numFramesPerSide = int(pow(2, textureMapping.Log2Width));
            int numFrames = numFramesPerSide * numFramesPerSide;
            int currentFrame = int(mod(t * textureMapping.Fps, numFrames));
            int currentFrameU = int(mod(currentFrame, numFramesPerSide));
            int currentFrameV = currentFrame / numFramesPerSide;
            uv.x += currentFrameU / numFramesPerSide;
            uv.y += currentFrameV / numFramesPerSide;
            break;
        }

        case TEXTURE_MAPPING_RANDOM:
        {
            // TODO: Haven't seen any non-zero values to test this with yet.
            break;
        }

        case TEXTURE_MAPPING_WS_ENVIRONMENT:
        {
            //TODO: this is just a copy of ENVIRONMENT
            uv = (reflect(viewVector, worldNormal).xy / 2.0) + vec2(0.5f, 0.5f);
            break;
        }

        case TEXTURE_MAPPING_BUMP_ENV:
        {
            //TODO: this is just a copy of ENVIRONMENT
            uv = (reflect(viewVector, worldNormal).xy / 2.0) + vec2(0.5f, 0.5f);
            break;
        }
    }

    return texture(sampler2D(diffuseTexture, Sampler), uv);
}

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

    vec3 materialAmbientColor = _MaterialConstants.Material.Ambient;
    vec3 materialDiffuseColor = _MaterialConstants.Material.Diffuse;
    if (_MeshConstants.HasHouseColor)
    {
        materialAmbientColor = _RenderItemConstantsPS.HouseColor;
        materialDiffuseColor = _RenderItemConstantsPS.HouseColor;
    }

    vec3 diffuseColor;
    vec3 specularColor;

    DoLighting(
        _GlobalLightingConstantsPS,
        in_WorldPosition,
        in_WorldNormal,
        materialAmbientColor,
        materialDiffuseColor,
        _MaterialConstants.Material.Specular,
        _MaterialConstants.Material.Shininess,
        _GlobalConstantsShared.CameraPosition,
        true,
        shadowVisibility,
        diffuseColor,
        specularColor);

    vec4 diffuseTextureColor;
    if (_MaterialConstants.Shading.TexturingEnabled)
    {
        vec3 v = CalculateViewVector(_GlobalConstantsShared.CameraPosition, in_WorldPosition);

        diffuseTextureColor = SampleTexture(
            in_WorldNormal, in_UV0, gl_FragCoord.xy,
            _MaterialConstants.Material.TextureMappingStage0,
            Texture0,
            v);

        if (_MaterialConstants.NumTextureStages > 1)
        {
            vec4 secondaryTextureColor = SampleTexture(
                in_WorldNormal, in_UV1, gl_FragCoord.xy,
                _MaterialConstants.Material.TextureMappingStage1,
                Texture1,
                v);

            switch (_MaterialConstants.Shading.SecondaryTextureColorBlend)
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

                case SECONDARY_TEXTURE_BLEND_ADD:
                    diffuseTextureColor = vec4(
                        diffuseTextureColor.xyz + secondaryTextureColor.xyz,
                        diffuseTextureColor.w);
                    break;
            }

            switch (_MaterialConstants.Shading.SecondaryTextureAlphaBlend)
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

        if (_MaterialConstants.Shading.AlphaTest)
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
    
    vec3 totalObjectLighting = saturate(diffuseColor + _MaterialConstants.Material.Emissive);

    vec3 objectColor = diffuseTextureColor.xyz;

    switch (_MaterialConstants.Shading.DiffuseLightingType)
    {
        case DIFFUSE_LIGHTING_MODULATE:
            objectColor *= totalObjectLighting;
            break;

        case DIFFUSE_LIGHTING_ADD:
            objectColor += totalObjectLighting;
            break;

        case DIFFUSE_LIGHTING_BUMP_ENV_MAP:
            //TODO
            break;
    }

    if (_MaterialConstants.Shading.SpecularEnabled)
    {
        objectColor += specularColor;
    }

    vec3 cloudColor = GetCloudColor(
        Global_CloudTexture,
        Sampler,
        in_CloudUV);

    out_Color = vec4(
        objectColor * cloudColor,
        _MaterialConstants.Material.Opacity * diffuseTextureColor.w);
}