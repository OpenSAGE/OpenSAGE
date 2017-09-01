#include "Mesh.hlsli"

struct VSInput
{
    float3 Position  : POSITION;
    float3 Normal    : NORMAL;
    float2 UV        : TEXCOORD0;

    uint VertexID : SV_VertexID;
};

struct MeshTransformConstants
{
    row_major float4x4 WorldViewProjection;
    row_major float4x4 World;
};

ConstantBuffer<MeshTransformConstants> MeshTransformCB : register(b0);

Buffer<uint> MaterialIndices : register(t0);

VSOutput main(VSInput input)
{
    VSOutput result;

    result.Position = mul(float4(input.Position, 1), MeshTransformCB.WorldViewProjection);
    result.WorldPosition = mul(input.Position, (float3x3) MeshTransformCB.World);

    result.Normal = mul(input.Normal, (float3x3) MeshTransformCB.World);
    result.UV = input.UV;

    // TODO: Make sure that material index is constant for all vertices in a triangle.
    result.MaterialIndex = MaterialIndices[input.VertexID];

    return result;
}