///////////////////////////////
// Structures
///////////////////////////////

struct VSInputSkinnedInstanced
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

    uint InstanceID : SV_InstanceID;
};

struct PSInputCommon
{
    float3 WorldPosition : TEXCOORD0;
    float3 WorldNormal   : TEXCOORD1;
    float2 UV0           : TEXCOORD2;
    float2 UV1           : TEXCOORD3;
};

struct VSOutputCommon
{
    float4 Position : SV_Position;
};

struct MeshTransformConstants
{
    row_major float4x4 ViewProjection;
    bool SkinningEnabled;
    uint NumBones;
};

///////////////////////////////
// Buffers
///////////////////////////////

ConstantBuffer<MeshTransformConstants> MeshTransformCB : register(b0);

StructuredBuffer<float4x3> SkinningBuffer : register(t0);

///////////////////////////////
// Functions
///////////////////////////////

void VSSkinnedInstanced(
    in VSInputSkinnedInstanced input, 
    out VSOutputCommon vsOutput,
    out PSInputCommon psInput)
{
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

    vsOutput.Position = mul(worldPosition, MeshTransformCB.ViewProjection);

    psInput.WorldPosition = worldPosition.xyz;

    psInput.WorldNormal = mul(input.Normal, (float3x3) world);

    psInput.UV0 = input.UV0;
    psInput.UV1 = input.UV1;
}