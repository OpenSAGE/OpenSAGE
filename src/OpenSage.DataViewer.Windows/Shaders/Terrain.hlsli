struct PSInputBase
{
    float3 WorldPosition : TEXCOORD0;
    float3 WorldNormal   : TEXCOORD1;
};

struct VSOutput : PSInputBase
{
    float4 Position : SV_Position;
};

struct PSInput : PSInputBase
{
    
};