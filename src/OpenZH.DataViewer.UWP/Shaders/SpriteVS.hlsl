#include "Sprite.hlsli"

PSInput main(uint vertexID : SV_VERTEXID)
{
	PSInput output;
	output.TexCoords = float2((vertexID << 1) & 2, vertexID & 2);
	output.Position = float4(output.TexCoords * float2(2, -2) + float2(-1, 1), 0, 1);
	output.Position.xy *= 0.9;
	return output;
}