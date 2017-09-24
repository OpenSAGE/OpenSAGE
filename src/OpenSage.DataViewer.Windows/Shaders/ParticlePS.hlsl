#include "Particle.hlsli"

Texture2D<float4> ParticleTexture : register(t0);
SamplerState LinearSampler : register(s0);

float4 main(PSInput input) : SV_TARGET
{
    return ParticleTexture.Sample(
        LinearSampler, 
        input.TexCoords);

    // TODO: Alpha test
}