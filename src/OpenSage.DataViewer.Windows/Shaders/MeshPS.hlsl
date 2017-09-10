#include "Mesh.hlsli"

struct LightingConstants
{
    float3 CameraPosition;
    float3 AmbientLightColor;
    float3 Light0Direction;
    float3 Light0Color;
};

ConstantBuffer<LightingConstants> LightingCB : register(b0);

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

Buffer<uint2> TextureIndices : register(t1);
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

    float3 lightDir = LightingCB.Light0Direction;

    float3 diffuse = saturate(dot(input.Normal, -lightDir)) * material.Diffuse;

    float3 v = normalize(LightingCB.CameraPosition - input.WorldPosition);
    float3 h = normalize(v - lightDir);
    float3 specular = saturate(dot(input.Normal, h)) * material.Shininess * material.Specular;

    float4 diffuseTextureColor;
    if (PerDrawCB.Texturing)
    {
        diffuseTextureColor = SampleTexture(
            0, input.Normal, input.UV0,
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

    float3 ambient = LightingCB.AmbientLightColor * material.Ambient;

    float3 totalObjectLighting = saturate(ambient + diffuse + material.Emissive);

    float3 color = (totalObjectLighting * diffuseTextureColor.rgb + specular) * LightingCB.Light0Color;

    float alpha = material.Opacity * diffuseTextureColor.a;

    return float4(color, alpha);
}