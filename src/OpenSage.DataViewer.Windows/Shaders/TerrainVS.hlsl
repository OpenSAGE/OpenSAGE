#include "Terrain.hlsli"

struct VSInput
{
    float3 Position : POSITION;
    float3 Normal   : NORMAL;
};

struct TransformConstants
{
    row_major float4x4 WorldViewProjection;
    row_major float4x4 World;
};

ConstantBuffer<TransformConstants> TransformCB : register(b0);

VSOutput main(VSInput input)
{
    VSOutput result;

    result.Position = mul(float4(input.Position, 1), TransformCB.WorldViewProjection);
    result.WorldPosition = mul(input.Position, (float3x3) TransformCB.World);

    result.WorldNormal = mul(input.Normal, (float3x3) TransformCB.World);

    return result;
}