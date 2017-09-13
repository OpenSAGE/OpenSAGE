#include "Terrain.hlsli"

struct LightingConstants
{
    float3 CameraPosition;
    float3 AmbientLightColor;
    float3 Light0Direction;
    float3 Light0Color;
};

ConstantBuffer<LightingConstants> LightingCB : register(b0);

struct TextureInfo
{
    uint CellSize;
};

Buffer<uint> TextureIndices : register(t0);
StructuredBuffer<TextureInfo> TextureDetails : register(t1);
Texture2D<float4> Textures[] : register(t2);

SamplerState Sampler : register(s0);

float4 main(PSInput input) : SV_TARGET
{
    float3 lightDir = LightingCB.Light0Direction;

    float3 diffuse = saturate(dot(input.WorldNormal, -lightDir))
        * float3(0.5, 0.5, 0.5);

    float3 ambient = LightingCB.AmbientLightColor;

    float3 totalObjectLighting = saturate(ambient + diffuse);

    uint textureIndex = TextureIndices[input.PrimitiveID];

    // TODO: Since all pixels in a primitive share the same textureIndex,
    // can we remove the call to NonUniformResourceIndex?
    Texture2D<float4> diffuseTexture = Textures[NonUniformResourceIndex(textureIndex)];
    TextureInfo textureInfo = TextureDetails[NonUniformResourceIndex(textureIndex)];
    float4 diffuseTextureColor = diffuseTexture.Sample(Sampler, input.UV / textureInfo.CellSize);

    float3 color = (totalObjectLighting * diffuseTextureColor.rgb) * LightingCB.Light0Color;

    return float4(color, 1);
}