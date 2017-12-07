#include "Sprite.hlsli"

PSInput main(float2 position : POSITION, float2 uv : TEXCOORD)
{
    PSInput output;
    
    output.Position = float4(position, 0, 1);
    output.TexCoords = uv;

    return output;
}