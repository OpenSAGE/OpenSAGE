#include "Mesh.hlsli"

struct VSInput
{
    float3 Position : POSITION;
    float3 Normal   : NORMAL;
    uint BoneIndex  : BLENDINDICES;
    float2 UV       : TEXCOORD0;

    uint VertexID : SV_VertexID;
};

struct MeshTransformConstants
{
    row_major float4x4 WorldViewProjection;
    row_major float4x4 World;
    bool SkinningEnabled;
};

ConstantBuffer<MeshTransformConstants> MeshTransformCB : register(b0);

#define MAX_BONES 72

struct SkinningConstants
{
    float4x3 Bones[MAX_BONES];
};

ConstantBuffer<SkinningConstants> SkinningCB : register(b1);

Buffer<uint> MaterialIndices : register(t0);

VSOutput main(VSInput input)
{
    VSOutput result;

    if (MeshTransformCB.SkinningEnabled)
    {
        float4x3 skinning = SkinningCB.Bones[input.BoneIndex];

        input.Position = mul(float4(input.Position, 1), skinning);
        input.Normal = mul(input.Normal, (float3x3) skinning);
    }

    result.Position = mul(float4(input.Position, 1), MeshTransformCB.WorldViewProjection);
    result.WorldPosition = mul(input.Position, (float3x3) MeshTransformCB.World);

    result.Normal = mul(input.Normal, (float3x3) MeshTransformCB.World);
    result.UV = input.UV;

    // TODO: Make sure that material index is constant for all vertices in a triangle.
    result.MaterialIndex = MaterialIndices[input.VertexID];

    return result;
}