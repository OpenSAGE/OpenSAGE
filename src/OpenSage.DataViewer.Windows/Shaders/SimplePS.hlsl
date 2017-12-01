#include "MeshCommonPS.hlsli"
#include "Simple.hlsli"

cbuffer MaterialConstants : register(b2)
{
    float3 ColorEmissive;
    float4 TexCoordTransform0;
};

Texture2D<float4> Texture0 : register(t0);

SamplerState Sampler : register(s0);

float4 main(VSOutputSimple input) : SV_Target
{
    float2 uv = input.TransferCommon.UV0 * TexCoordTransform0.xy + TimeInSeconds * TexCoordTransform0.zw;

    float4 color = float4(ColorEmissive, 1);

    color *= Texture0.Sample(Sampler, uv);

    // TODO: Fog.

    return color;
}