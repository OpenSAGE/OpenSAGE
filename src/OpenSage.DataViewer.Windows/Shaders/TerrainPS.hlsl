#include "Terrain.hlsli"

struct Light
{
    float3 Ambient;
    float3 Color;
    float3 Direction;
};

#define NUM_LIGHTS 3

struct LightingConstants
{
    float3 CameraPosition;
    Light Lights[NUM_LIGHTS];
};

ConstantBuffer<LightingConstants> LightingCB : register(b0);

struct TextureInfo
{
    uint CellSize;
};

#define BLEND_DIRECTION_TOWARDS_RIGHT     1
#define BLEND_DIRECTION_TOWARDS_TOP       2
#define BLEND_DIRECTION_TOWARDS_TOP_LEFT  4
#define BLEND_DIRECTION_TOWARDS_TOP_RIGHT 8

Texture2D<uint4> TileData : register(t0);

StructuredBuffer<TextureInfo> TextureDetails : register(t1);
Texture2D<float4> Textures[] : register(t2);

SamplerState Sampler : register(s0);

float3 SampleTexture(
    int textureIndex,
    float2 uv)
{
    // TODO: Since all pixels in a primitive share the same textureIndex,
    // can we remove the call to NonUniformResourceIndex?
    Texture2D<float4> diffuseTexture = Textures[NonUniformResourceIndex(textureIndex)];
    TextureInfo textureInfo = TextureDetails[textureIndex];

    float2 scaledUV = uv / textureInfo.CellSize;

    float4 diffuseTextureColor = diffuseTexture.Sample(
        Sampler,
        float2(scaledUV.x, 1 - scaledUV.y));

    return diffuseTextureColor.rgb;
}

float3 SampleBlendedTextures(float2 uv)
{
    uint4 tileDatum = TileData.Load(int3(uv, 0));

    float3 baseTextureColor = SampleTexture(tileDatum.x, uv);

    float3 secondaryTextureColor = SampleTexture(tileDatum.y, uv);

    float blendFactor = 0;
    float2 fracUV = frac(uv);

    if (tileDatum.w == 1) // Reverse direction
    {
        fracUV.x = 1 - fracUV.x;
        fracUV.y = 1 - fracUV.y;
    }

    switch (tileDatum.z)
    {
    case BLEND_DIRECTION_TOWARDS_RIGHT:
        blendFactor = fracUV.x;
        break;

    case BLEND_DIRECTION_TOWARDS_TOP:
        blendFactor = fracUV.y;
        break;

    case BLEND_DIRECTION_TOWARDS_TOP_LEFT:
        //fracUV.y = 1 - fracUV.y;
        blendFactor = max(1 - fracUV.x, fracUV.y);
        break;

    case BLEND_DIRECTION_TOWARDS_TOP_RIGHT:
        //fracUV.x = 1 - fracUV.x;
        blendFactor = max(fracUV.x, fracUV.y);
        break;
    }

    return baseTextureColor * (1 - blendFactor) + secondaryTextureColor * blendFactor;
}

float4 main(PSInput input) : SV_TARGET
{
    float3 color = float3(0, 0, 0);

    for (int i = 0; i < NUM_LIGHTS; i++)
    {
        Light light = LightingCB.Lights[i];

        float diffuse = saturate(dot(input.WorldNormal, -light.Direction));

        color += light.Ambient + float3(diffuse, diffuse, diffuse) * light.Color;
    }

    color = saturate(color);

    float3 textureColor = SampleBlendedTextures(input.UV);

    return float4(
        color * textureColor,
        1);
}