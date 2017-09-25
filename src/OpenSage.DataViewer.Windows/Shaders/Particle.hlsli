struct PSInput
{
    float4 Position  : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
    float3 Color     : TEXCOORD1;
    float  Alpha     : TEXCOORD2;
};