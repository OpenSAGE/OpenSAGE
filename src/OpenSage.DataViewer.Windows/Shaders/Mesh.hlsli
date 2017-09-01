struct PSInputBase
{
    float3 WorldPosition : TEXCOORD0;
    float3 Normal        : TEXCOORD1;
    float2 UV            : TEXCOORD2;
    uint   MaterialIndex : TEXCOORD3;
};

struct VSOutput : PSInputBase
{
    float4 Position : SV_Position;
};

struct PSInput : PSInputBase
{
    uint PrimitiveID : SV_PrimitiveID;
};