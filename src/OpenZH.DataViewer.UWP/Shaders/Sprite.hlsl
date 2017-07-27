struct PSInput
{
  float4 Position : SV_POSITION;
	float2 TexCoords : TEXCOORD;
};

PSInput VSMain(uint vertexID : SV_VERTEXID)
{
	PSInput output;
	output.TexCoords = float2((vertexID << 1) & 2, vertexID & 2);
	output.Position = float4(output.TexCoords * float2(2, -2) + float2(-1, 1), 0, 1);
	output.Position.xy *= 0.9;
	return output;
}

Texture2D<float4> BaseTexture : register(t0);
SamplerState LinearSampler : register(s0);

float4 PSMain(PSInput input) : SV_TARGET
{
  return BaseTexture.SampleLevel(LinearSampler, input.TexCoords, 0);
}