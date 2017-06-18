struct VSInput
{
	float3 position : POSITION;
	//float3 normal : NORMAL;
};

struct PSInput
{
	float4 position : SV_POSITION;
};

cbuffer Globals : register(b0)
{
	float4x4 WorldViewProj;
};

PSInput VSMain(VSInput input)
{
	PSInput result;

	result.position = mul(float4(input.position, 1), WorldViewProj);

	return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
	return float4(0, 1, 0, 1);
}