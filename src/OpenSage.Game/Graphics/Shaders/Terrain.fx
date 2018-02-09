#include "Common.hlsli"

#define LIGHTING_CONSTANTS_VS_REGISTER b3
#define LIGHTING_CONSTANTS_PS_REGISTER b4
#include "Lighting.hlsli"

#define CLOUD_TEXTURE_REGISTER t4
#include "Clouds.hlsli"

struct PSInputBase
{
    float3 WorldPosition : TEXCOORD0;
    float3 WorldNormal   : TEXCOORD1;
    float2 UV            : TEXCOORD2;
    float2 CloudUV       : TEXCOORD3;
};

struct VSOutput : PSInputBase
{
    float4 Position : SV_Position;
};

struct PSInput : PSInputBase
{

};

struct VSInput
{
    float3 Position : POSITION;
    float3 Normal   : NORMAL;
    float2 UV       : TEXCOORD;
};

cbuffer RenderItemConstantsVS : register(b2)
{
    row_major float4x4 World;
};

VSOutput VS(VSInput input)
{
    VSOutput result;

    float3 worldPosition = mul(input.Position, (float3x3) World);

    result.Position = mul(float4(worldPosition, 1), ViewProjection);
    result.WorldPosition = worldPosition;

    result.WorldNormal = mul(input.Normal, (float3x3) World);

    result.UV = input.UV;

    result.CloudUV = GetCloudUV(result.WorldPosition);

    return result;
}

cbuffer TerrainMaterialConstants : register(b5)
{
    float2 MapBorderWidth;
    float2 MapSize;
    bool IsMacroTextureStretched;
};

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
    uint TextureIndex;
    uint CellSize;
};

StructuredBuffer<TextureInfo> TextureDetails : register(t2);

Texture2DArray<float4> Textures : register(t3);

Texture2D<float4> MacroTexture : register(t5);

SamplerState Sampler : register(s0);

float3 SampleTexture(
    int textureIndex,
    float2 uv,
    float2 ddxUV,
    float2 ddyUV)
{
    TextureInfo textureInfo = TextureDetails[textureIndex];

    float2 scaledUV = uv / textureInfo.CellSize;

    // Can't use standard Sample because UV is scaled by texture CellSize,
    // and that doesn't work for divergent texture lookups.
    float4 diffuseTextureColor = Textures.SampleGrad(
        Sampler,
        float3(scaledUV, textureInfo.TextureIndex),
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
    bool flipped = (blendFlags & 1) == 1;
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
        float2 uvXTop = lerp(cliffInfo.TopLeftUV, cliffInfo.TopRightUV, fracUV.x);

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

float2 GetMacroTextureUV(float3 worldPosition)
{
    if (IsMacroTextureStretched)
    {
        return (worldPosition.xy + MapBorderWidth) / float2(MapSize.x, -MapSize.y);
    }
    else
    {
        float macroTextureScale = 1 / 660.0;
        return worldPosition.xy * float2(macroTextureScale, -macroTextureScale);
    }
}

float4 PS(PSInput input) : SV_Target
{
    LightingParameters lightingParams;
    lightingParams.WorldNormal = input.WorldNormal;
    lightingParams.MaterialAmbient = float3(1, 1, 1);
    lightingParams.MaterialDiffuse = float3(1, 1, 1);

    float3 diffuseColor;
    float3 specularColor;
    DoLighting(lightingParams, diffuseColor, specularColor);

    float3 textureColor = SampleBlendedTextures(input.UV);

    float3 cloudColor = GetCloudColor(Sampler, input.CloudUV);

    float2 macroTextureUV = GetMacroTextureUV(input.WorldPosition);
    float3 macroTextureColor = MacroTexture.Sample(Sampler, macroTextureUV);

    return float4(
        diffuseColor * textureColor * cloudColor * macroTextureColor,
        1);
}