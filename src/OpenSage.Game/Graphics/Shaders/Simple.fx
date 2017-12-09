#include "Common.hlsli"
#include "MeshCommon.hlsli"

struct VSOutputSimple
{
    VSOutputCommon VSOutput;
    PSInputCommon TransferCommon;
};

VSOutputSimple VS(VSInputSkinnedInstanced input)
{
    VSOutputSimple result;

    VSSkinnedInstanced(input, result.VSOutput, result.TransferCommon);

    return result;
}

cbuffer MaterialConstants : register(b2)
{
    float4 ColorEmissive;
    float4 TexCoordTransform_0;
};

Texture2D<float4> Texture_0 : register(t0);
SamplerState Sampler : register(s0);

float4 PS(VSOutputSimple input) : SV_Target
{
    float2 uv = input.TransferCommon.UV0 * TexCoordTransform_0.xy + TimeInSeconds * TexCoordTransform_0.zw;

    float4 color = float4(ColorEmissive.rgb, 1);

    color *= Texture_0.Sample(Sampler, uv);

    // TODO: Fog.

    return color;
}