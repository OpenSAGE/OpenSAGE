#include "Sprite.hlsli"

Texture2D<float4> BaseTexture : register(t0);
SamplerState LinearSampler : register(s0);

cbuffer TextureCB : register(b0)
{
    uint MipMapLevel;
};

float4 main(PSInput input) : SV_TARGET
{
    return BaseTexture.SampleLevel(
        LinearSampler, 
        input.TexCoords, 
        MipMapLevel);
}