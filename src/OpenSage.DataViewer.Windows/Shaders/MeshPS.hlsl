#include "Mesh.hlsli"

#define LIGHTING_CB_REGISTER b0
#define SPECULAR_ENABLED
#include "Lighting.hlsli"

struct PerDrawConstants
{
    uint PrimitiveOffset;
    uint NumTextureStages;
    bool AlphaTest;
    bool Texturing;
    float TimeInSeconds;
};

ConstantBuffer<PerDrawConstants> PerDrawCB : register(b1);

#define TEXTURE_MAPPING_UV            0
#define TEXTURE_MAPPING_ENVIRONMENT   1
#define TEXTURE_MAPPING_LINEAR_OFFSET 2

struct TextureMapping
{
    uint MappingType;
    float2 UVPerSec;
    float2 UVScale;
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

StructuredBuffer<uint2> TextureIndices : register(t1);
Texture2D<float4> Textures[] : register(t2);

SamplerState Sampler : register(s0);

float4 SampleTexture(
    uint primitiveID, float3 worldNormal, float2 uv,
    TextureMapping textureMapping,
    uint textureStage,
    float3 viewVector)
{
    uint2 textureIndices = TextureIndices[PerDrawCB.PrimitiveOffset + primitiveID];
    uint textureIndex = textureIndices[textureStage];

    // TODO: Since all pixels in a primitive share the same textureIndex,
    // can we remove the call to NonUniformResourceIndex?
    Texture2D<float4> diffuseTexture = Textures[NonUniformResourceIndex(textureIndex)];

    switch (textureMapping.MappingType)
    {
    case TEXTURE_MAPPING_UV:
        uv = float2(uv.x, 1 - uv.y);
        break;

    case TEXTURE_MAPPING_ENVIRONMENT:
        uv = (reflect(viewVector, worldNormal).xy / 2.0f) + float2(0.5f, 0.5f);
        break;

    case TEXTURE_MAPPING_LINEAR_OFFSET:
        float2 offset = textureMapping.UVPerSec * PerDrawCB.TimeInSeconds;
        uv = float2(uv.x, 1 - uv.y) + offset;
        uv *= textureMapping.UVScale;
        break;
    }

    return diffuseTexture.Sample(Sampler, uv);
}

float4 main(PSInput input) : SV_TARGET
{
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
    if (PerDrawCB.Texturing)
    {
        float3 v = CalculateViewVector(input.WorldPosition);

        diffuseTextureColor = SampleTexture(
            input.PrimitiveID, input.Normal, input.UV0,
            material.TextureMappingStage0,
            0, v);

        if (PerDrawCB.NumTextureStages > 1)
        {
            // TODO: Is this the right way to combine texture stages?
            diffuseTextureColor += SampleTexture(
                input.PrimitiveID, input.Normal, input.UV1,
                material.TextureMappingStage1,
                1, v);
        }

        if (PerDrawCB.AlphaTest)
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

    return float4(
        (totalObjectLighting * diffuseTextureColor.rgb) + specularColor, 
        material.Opacity * diffuseTextureColor.a);
}