#include "Sprite.hlsli"

PSInput main(uint vertexID : SV_VERTEXID)
{
	PSInput output;
  // Texture coordinates range [0, 2], but only [0, 1] appears on screen.
	output.TexCoords = float2((vertexID << 1) & 2, vertexID & 2);
	output.Position = float4(output.TexCoords * float2(2, -2) + float2(-1, 1), 0, 1);
	return output;
}