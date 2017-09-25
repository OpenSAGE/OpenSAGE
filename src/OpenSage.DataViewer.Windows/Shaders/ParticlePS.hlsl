#include "Particle.hlsli"

Texture2D<float4> ParticleTexture : register(t0);
SamplerState LinearSampler : register(s0);

float4 main(PSInput input) : SV_TARGET
{
    float4 texColor = ParticleTexture.Sample(
        LinearSampler, 
        input.TexCoords);

    texColor.rgb *= input.Color;

    texColor.a *= input.Alpha;

    return texColor;

    // TODO: Alpha test
}