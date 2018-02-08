#include "Clouds.hlsli"

///////////////////////////////
// Structures
///////////////////////////////

struct PSInputCommon
{
    float3 WorldPosition : TEXCOORD0;
    float3 WorldNormal   : TEXCOORD1;
    float2 UV0           : TEXCOORD2;
    float2 UV1           : TEXCOORD3;
    float2 CloudUV       : TEXCOORD4;
};

struct VSOutputCommon
{
    float4 Position : SV_Position;
};

///////////////////////////////
// Structures
///////////////////////////////

struct VSInputSkinned
{
    float3 Position : POSITION;
    float3 Normal   : NORMAL;
    float3 Tangent  : TEXCOORD0;
    float3 Binormal : TEXCOORD1;
    uint BoneIndex  : TEXCOORD2;

    float2 UV0      : TEXCOORD3;
    float2 UV1      : TEXCOORD4;
};

///////////////////////////////
// Buffers
///////////////////////////////

cbuffer MeshConstants : register(MESH_CONSTANTS_REGISTER)
{
    bool SkinningEnabled;
    uint NumBones;
};

cbuffer RenderItemConstantsVS : register(RENDER_ITEM_CONSTANTS_VS_REGISTER)
{
    row_major float4x4 World;
};

StructuredBuffer<float4x3> SkinningBuffer : register(t0);

///////////////////////////////
// Functions
///////////////////////////////

void VSSkinnedInstanced(
    in VSInputSkinned input,
    out VSOutputCommon vsOutput,
    out PSInputCommon psInput)
{
    if (SkinningEnabled)
    {
        float4x3 skinning = SkinningBuffer[input.BoneIndex];

        input.Position = mul(float4(input.Position, 1), skinning);
        input.Normal = mul(input.Normal, (float3x3) skinning);
    }

    float4 worldPosition = mul(float4(input.Position, 1), World);

    vsOutput.Position = mul(worldPosition, ViewProjection);

    psInput.WorldPosition = worldPosition.xyz;

    psInput.WorldNormal = mul(input.Normal, (float3x3) World);

    psInput.UV0 = input.UV0;
    psInput.UV1 = input.UV1;

    psInput.CloudUV = GetCloudUV(psInput.WorldPosition);
}