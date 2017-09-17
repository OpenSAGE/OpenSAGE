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

#define BLEND_DIRECTION_TOWARDS_RIGHT     1
#define BLEND_DIRECTION_TOWARDS_TOP       2
#define BLEND_DIRECTION_TOWARDS_TOP_RIGHT 4
#define BLEND_DIRECTION_TOWARDS_TOP_LEFT  8

Texture2D<uint4> TileData : register(t0);

struct CliffInfo
{
    float2 BottomLeftUV;
    float2 BottomRightUV;
    float2 TopRightUV;
    float2 TopLeftUV;
};

StructuredBuffer<CliffInfo> CliffDetails : register(t1);

struct TextureInfo
{
    uint CellSize;
};

StructuredBuffer<TextureInfo> TextureDetails : register(t2);
Texture2D<float4> Textures[] : register(t3);

SamplerState Sampler : register(s0);

float3 SampleTexture(
    int textureIndex,
    float2 uv,
    float2 ddxUV,
    float2 ddyUV)
{
    // TODO: Since all pixels in a primitive share the same textureIndex,
    // can we remove the call to NonUniformResourceIndex?
    Texture2D<float4> diffuseTexture = Textures[NonUniformResourceIndex(textureIndex)];
    TextureInfo textureInfo = TextureDetails[textureIndex];

    float2 scaledUV = uv / textureInfo.CellSize;

    // Can't use standard Sample because UV is scaled by texture CellSize,
    // and that doesn't work for divergent texture lookups.
    float4 diffuseTextureColor = diffuseTexture.SampleGrad(
        Sampler,
        float2(scaledUV.x, -scaledUV.y),
        ddxUV / textureInfo.CellSize,
        ddyUV / textureInfo.CellSize);

    return diffuseTextureColor.rgb;
}

float CalculateDiagonalBlendFactor(float2 fracUV, bool twoSided)
{
    return twoSided
        ? 1 - saturate((fracUV.x + fracUV.y) - 1)
        : saturate(1 - (fracUV.x + fracUV.y));
}

float CalculateBlendFactor(
    uint blendDirection,
    uint blendFlags,
    float2 fracUV)
{
    bool flipped  = (blendFlags & 1) == 1;
    bool twoSided = (blendFlags & 2) == 2;

    if (flipped)
    {
        switch (blendDirection)
        {
        case BLEND_DIRECTION_TOWARDS_RIGHT:
            fracUV.x = 1 - fracUV.x;
            break;

        case BLEND_DIRECTION_TOWARDS_TOP:
        case BLEND_DIRECTION_TOWARDS_TOP_RIGHT:
        case BLEND_DIRECTION_TOWARDS_TOP_LEFT:
            fracUV.y = 1 - fracUV.y;
            break;
        }
    }

    float blendFactor = 0;

    switch (blendDirection)
    {
    case BLEND_DIRECTION_TOWARDS_RIGHT:
        blendFactor = fracUV.x;
        break;

    case BLEND_DIRECTION_TOWARDS_TOP:
        blendFactor = fracUV.y;
        break;

    case BLEND_DIRECTION_TOWARDS_TOP_RIGHT:
        fracUV = float2(1, 1) - fracUV;
        blendFactor = CalculateDiagonalBlendFactor(fracUV, twoSided);
        break;

    case BLEND_DIRECTION_TOWARDS_TOP_LEFT:
        fracUV.y = 1 - fracUV.y;
        blendFactor = CalculateDiagonalBlendFactor(fracUV, twoSided);
        break;
    }

    return blendFactor;
}

float3 SampleBlendedTextures(float2 uv)
{
    uint4 tileDatum = TileData.Load(int3(uv, 0));

    float2 fracUV = frac(uv);

    uint cliffTextureIndex = tileDatum.z;
    if (cliffTextureIndex != 0)
    {
        CliffInfo cliffInfo = CliffDetails[cliffTextureIndex - 1];

        float2 uvXBottom = lerp(cliffInfo.BottomLeftUV, cliffInfo.BottomRightUV, fracUV.x);
        float2 uvXTop    = lerp(cliffInfo.TopLeftUV, cliffInfo.TopRightUV, fracUV.x);

        uv = lerp(uvXBottom, uvXTop, fracUV.y);
    }

    float2 ddxUV = ddx(uv);
    float2 ddyUV = ddy(uv);

    uint packedTextureIndices = tileDatum.x;
    uint textureIndex0 = packedTextureIndices & 0xFF;
    uint textureIndex1 = (packedTextureIndices >> 8) & 0xFF;
    uint textureIndex2 = (packedTextureIndices >> 16) & 0xFF;

    uint packedBlendInfo = tileDatum.y;
    uint blendDirection1 = packedBlendInfo & 0xFF;
    uint blendFlags1 = (packedBlendInfo >> 8) & 0xFF;
    uint blendDirection2 = (packedBlendInfo >> 16) & 0xFF;
    uint blendFlags2 = (packedBlendInfo >> 24) & 0xFF;

    float3 textureColor0 = SampleTexture(textureIndex0, uv, ddxUV, ddyUV);
    float3 textureColor1 = SampleTexture(textureIndex1, uv, ddxUV, ddyUV);
    float3 textureColor2 = SampleTexture(textureIndex2, uv, ddxUV, ddyUV);

    float blendFactor1 = CalculateBlendFactor(blendDirection1, blendFlags1, fracUV);
    float blendFactor2 = CalculateBlendFactor(blendDirection2, blendFlags2, fracUV);

    return 
        lerp(
            lerp(
                textureColor0, 
                textureColor1, 
                blendFactor1), 
            textureColor2, 
            blendFactor2);
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