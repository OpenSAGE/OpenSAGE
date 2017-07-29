struct PSInput
{
  float4 Position      : SV_POSITION;
  float3 WorldPosition : TEXCOORD0;
  float3 Normal        : TEXCOORD1;
	float2 UV            : TEXCOORD2;
};