#include "Terrain.hlsli"

struct VSInput
{
    float3 Position : POSITION;
    float3 Normal   : NORMAL;
    float2 UV       : TEXCOORD;
};

cbuffer TransformCB : register(b0)
{
    row_major float4x4 WorldViewProjection;
    row_major float4x4 World;
};

VSOutput main(VSInput input)
{
    VSOutput result;

    result.Position = mul(float4(input.Position, 1), WorldViewProjection);
    result.WorldPosition = mul(input.Position, (float3x3) World);

    result.WorldNormal = mul(input.Normal, (float3x3) World);

    result.UV = input.UV;

    return result;
}