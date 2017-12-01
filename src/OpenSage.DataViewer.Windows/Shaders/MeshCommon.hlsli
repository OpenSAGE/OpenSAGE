///////////////////////////////
// Structures
///////////////////////////////

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