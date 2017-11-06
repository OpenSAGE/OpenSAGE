#include "Mesh.hlsli"

#define LIGHTING_CB_REGISTER b0
#define SPECULAR_ENABLED
#include "Lighting.hlsli"

struct PerDrawConstants
{
    uint ShadingConfigurationID;
    uint PrimitiveOffset;
    uint NumTextureStages;
    float TimeInSeconds;
    float2 ViewportSize;
};

ConstantBuffer<PerDrawConstants> PerDrawCB : register(b1);

#define TEXTURE_MAPPING_UV                 0
#define TEXTURE_MAPPING_ENVIRONMENT        1
#define TEXTURE_MAPPING_LINEAR_OFFSET      2
#define TEXTURE_MAPPING_ROTATE             3
#define TEXTURE_MAPPING_SINE_LINEAR_OFFSET 4
#define TEXTURE_MAPPING_SCREEN             5
#define TEXTURE_MAPPING_SCALE              6

struct TextureMapping
{
    uint MappingType;
    float2 UVPerSec;
    float2 UVScale;
    float2 UVCenter;
    float2 UVAmplitude;
    float2 UVFrequency;
    float2 UVPhase;
    float Speed;
};

struct VertexMaterial
{
    float3 Ambient;
    float3 Diffuse;
    float3 Specular;
    float Shininess;
    float3 Emissive;
    float Opacity;

    TextureMapping TextureMappingStage0;
    TextureMapping TextureMappingStage1;
};

StructuredBuffer<VertexMaterial> Materials : register(t0);

#define DIFFUSE_LIGHTING_DISABLE  0
#define DIFFUSE_LIGHTING_MODULATE 1
#define DIFFUSE_LIGHTING_ADD      2

#define SECONDARY_TEXTURE_BLEND_DISABLE   0
#define SECONDARY_TEXTURE_BLEND_DETAIL    1
#define SECONDARY_TEXTURE_BLEND_SCALE     2
#define SECONDARY_TEXTURE_BLEND_INV_SCALE 3

struct ShadingConfiguration
{
    uint DiffuseLightingType;
    bool SpecularEnabled;
    bool TexturingEnabled;
    uint SecondaryTextureColorBlend;
    uint SecondaryTextureAlphaBlend;
    bool AlphaTest;
};

StructuredBuffer<ShadingConfiguration> ShadingConfigurations : register(t1);

StructuredBuffer<uint2> TextureIndices : register(t2);
Texture2D<float4> Textures[] : register(t3);

SamplerState Sampler : register(s0);

#define TWO_PI (2 * 3.1415926535)

float4 SampleTexture(
    uint primitiveID, float3 worldNormal, float2 uv, float2 screenPosition,
    TextureMapping textureMapping,
    uint textureStage,
    float3 viewVector)
{
    uint2 textureIndices = TextureIndices[PerDrawCB.PrimitiveOffset + primitiveID];
    uint textureIndex = textureIndices[textureStage];

    // TODO: Since all pixels in a primitive share the same textureIndex,
    // can we remove the call to NonUniformResourceIndex?
    Texture2D<float4> diffuseTexture = Textures[NonUniformResourceIndex(textureIndex)];

    float t = PerDrawCB.TimeInSeconds;

    switch (textureMapping.MappingType)
    {
    case TEXTURE_MAPPING_UV:
        uv = float2(uv.x, 1 - uv.y);
        break;

    case TEXTURE_MAPPING_ENVIRONMENT:
        uv = (reflect(viewVector, worldNormal).xy / 2.0f) + float2(0.5f, 0.5f);
        break;

    case TEXTURE_MAPPING_LINEAR_OFFSET:
        float2 offset = textureMapping.UVPerSec * t;
        uv = float2(uv.x, 1 - uv.y) + offset;
        uv *= textureMapping.UVScale;
        break;

    case TEXTURE_MAPPING_ROTATE:
        float angle = textureMapping.Speed * t * TWO_PI;
        float s = sin(angle);
        float c = cos(angle);

        uv -= textureMapping.UVCenter;

        float2 rotatedPoint = float2(
            uv.x * c - uv.y * s,
            uv.x * s + uv.y * c);

        uv = rotatedPoint + textureMapping.UVCenter;

        uv *= textureMapping.UVScale;

        break;

    case TEXTURE_MAPPING_SINE_LINEAR_OFFSET:
        uv.x += textureMapping.UVAmplitude.x * sin(textureMapping.UVFrequency.x * t * TWO_PI + textureMapping.UVPhase.x * TWO_PI);
        uv.y += textureMapping.UVAmplitude.y * sin(textureMapping.UVFrequency.y * t * TWO_PI);
        break;

    case TEXTURE_MAPPING_SCREEN:
        uv = screenPosition / PerDrawCB.ViewportSize * textureMapping.UVScale;
        break;

    case TEXTURE_MAPPING_SCALE:
        uv *= textureMapping.UVScale;
        break;
    }

    return diffuseTexture.Sample(Sampler, uv);
}

float4 main(PSInput input) : SV_TARGET
{
    ShadingConfiguration shadingConfiguration = ShadingConfigurations[PerDrawCB.ShadingConfigurationID];

    VertexMaterial material = Materials[input.MaterialIndex];

    LightingParameters lightingParams;
    lightingParams.WorldPosition = input.WorldPosition;
    lightingParams.WorldNormal = input.Normal;
    lightingParams.MaterialAmbient = material.Ambient;
    lightingParams.MaterialDiffuse = material.Diffuse;
    lightingParams.MaterialSpecular = material.Specular;
    lightingParams.MaterialShininess = material.Shininess;

    float3 diffuseColor;
    float3 specularColor;
    DoLighting(lightingParams, diffuseColor, specularColor);

    float4 diffuseTextureColor;
    if (shadingConfiguration.TexturingEnabled)
    {
        float3 v = CalculateViewVector(input.WorldPosition);

        diffuseTextureColor = SampleTexture(
            input.PrimitiveID, input.Normal, input.UV0, input.ScreenPosition.xy,
            material.TextureMappingStage0,
            0, v);

        if (PerDrawCB.NumTextureStages > 1)
        {
            float4 secondaryTextureColor = SampleTexture(
                input.PrimitiveID, input.Normal, input.UV1, input.ScreenPosition.xy,
                material.TextureMappingStage1,
                1, v);

            switch (shadingConfiguration.SecondaryTextureColorBlend)
            {
            case SECONDARY_TEXTURE_BLEND_DETAIL:
                diffuseTextureColor.rgb = secondaryTextureColor.rgb;
                break;

            case SECONDARY_TEXTURE_BLEND_SCALE:
                diffuseTextureColor.rgb *= secondaryTextureColor.rgb;
                break;

            case SECONDARY_TEXTURE_BLEND_INV_SCALE:
                diffuseTextureColor.rgb += (float3(1, 1, 1) - diffuseTextureColor.rgb) * secondaryTextureColor.rgb;
                break;
            }

            switch (shadingConfiguration.SecondaryTextureAlphaBlend)
            {
            case SECONDARY_TEXTURE_BLEND_DETAIL:
                diffuseTextureColor.a = secondaryTextureColor.a;
                break;

            case SECONDARY_TEXTURE_BLEND_SCALE:
                diffuseTextureColor.a *= secondaryTextureColor.a;
                break;

            case SECONDARY_TEXTURE_BLEND_INV_SCALE:
                diffuseTextureColor.a += (1 - diffuseTextureColor.a) * secondaryTextureColor.a;
                break;
            }
        }

        if (shadingConfiguration.AlphaTest)
        {
            const float alphaTestThreshold = 0x60 / (float) 0xFF;
            if (diffuseTextureColor.a < alphaTestThreshold)
            {
                discard;
            }
        }
    }
    else
    {
        diffuseTextureColor = float4(1, 1, 1, 1);
    }

    float3 totalObjectLighting = saturate(diffuseColor + material.Emissive);

    float3 objectColor = diffuseTextureColor.rgb;

    switch (shadingConfiguration.DiffuseLightingType)
    {
    case DIFFUSE_LIGHTING_MODULATE:
        objectColor *= totalObjectLighting;
        break;

    case DIFFUSE_LIGHTING_ADD:
        objectColor += totalObjectLighting;
        break;
    }

    if (shadingConfiguration.SpecularEnabled)
    {
        objectColor += specularColor;
    }

    return float4(
        objectColor,
        material.Opacity * diffuseTextureColor.a);
}