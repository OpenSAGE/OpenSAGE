#include "CommonVS.hlsli"
#include "Terrain.hlsli"

struct VSInput
{
    float3 Position : POSITION;
    float3 Normal   : NORMAL;
    float2 UV       : TEXCOORD;
};

cbuffer RenderItemConstantsVS : register(b1)
{
    row_major float4x4 World;
};

VSOutput main(VSInput input)
{
    VSOutput result;

    float3 worldPosition = mul(input.Position, (float3x3) World);

    result.Position = mul(float4(worldPosition, 1), ViewProjection);
    result.WorldPosition = worldPosition;

    result.WorldNormal = mul(input.Normal, (float3x3) World);

    result.UV = input.UV;

    return result;
}