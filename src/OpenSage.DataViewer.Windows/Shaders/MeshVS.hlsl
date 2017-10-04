#include "Mesh.hlsli"

struct VSInput
{
    float3 Position : POSITION;
    float3 Normal   : NORMAL;
    uint BoneIndex  : BLENDINDICES;
    float2 UV0      : TEXCOORD0;
    float2 UV1      : TEXCOORD1;

    float4 World0   : TEXCOORD2;
    float4 World1   : TEXCOORD3;
    float4 World2   : TEXCOORD4;
    float4 World3   : TEXCOORD5;

    uint VertexID   : SV_VertexID;
    uint InstanceID : SV_InstanceID;
};

struct MeshTransformConstants
{
    row_major float4x4 ViewProjection;
    bool SkinningEnabled;
    uint NumBones;
};

ConstantBuffer<MeshTransformConstants> MeshTransformCB : register(b0);

StructuredBuffer<float4x3> SkinningBuffer : register(t0);

StructuredBuffer<uint> MaterialIndices : register(t1);

VSOutput main(VSInput input)
{
    VSOutput result;

    if (MeshTransformCB.SkinningEnabled)
    {
        uint skinningMatrixIndex = MeshTransformCB.NumBones * input.InstanceID + input.BoneIndex;
        float4x3 skinning = SkinningBuffer[skinningMatrixIndex];

        input.Position = mul(float4(input.Position, 1), skinning);
        input.Normal = mul(input.Normal, (float3x3) skinning);
    }

    matrix world;
    world[0] = input.World0;
    world[1] = input.World1;
    world[2] = input.World2;
    world[3] = input.World3;

    float4 worldPosition = mul(float4(input.Position, 1), world);

    result.Position = mul(worldPosition, MeshTransformCB.ViewProjection);
    result.WorldPosition = worldPosition.xyz;

    result.Normal = mul(input.Normal, (float3x3) world);

    result.UV0 = input.UV0;
    result.UV1 = input.UV1;

    // TODO: Make sure that material index is constant for all vertices in a triangle.
    result.MaterialIndex = MaterialIndices[input.VertexID];

    return result;
}