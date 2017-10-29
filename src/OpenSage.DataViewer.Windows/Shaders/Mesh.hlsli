struct PSInputBase
{
    float3 WorldPosition : TEXCOORD0;
    float3 Normal        : TEXCOORD1;
    float2 UV0           : TEXCOORD2;
    float2 UV1           : TEXCOORD3;
    uint   MaterialIndex : TEXCOORD4;
};

struct VSOutput : PSInputBase
{
    float4 Position : SV_Position;
};

struct PSInput : PSInputBase
{
    uint PrimitiveID : SV_PrimitiveID;
    float4 ScreenPosition : SV_Position;
};