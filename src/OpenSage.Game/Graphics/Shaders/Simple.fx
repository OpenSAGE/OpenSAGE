#include "Common.hlsli"

#define LIGHTING_CONSTANTS_VS_REGISTER b4
#define LIGHTING_CONSTANTS_PS_REGISTER b10
#include "Lighting.hlsli"

#define MESH_CONSTANTS_REGISTER b2
#define RENDER_ITEM_CONSTANTS_VS_REGISTER b3
#define CLOUD_TEXTURE_REGISTER t2
#include "MeshCommon.hlsli"

struct VSOutputSimple
{
    VSOutputCommon VSOutput;
    PSInputCommon TransferCommon;
};

VSOutputSimple VS(VSInputSkinned input)
{
    VSOutputSimple result;

    VSSkinnedInstanced(input, result.VSOutput, result.TransferCommon);

    return result;
}

cbuffer MaterialConstants : register(b5)
{
    float4 ColorEmissive;
    float4 TexCoordTransform_0;
};

Texture2D<float4> Texture_0 : register(t1);
SamplerState Sampler : register(s0);

float4 PS(VSOutputSimple input) : SV_Target
{
    float2 uv = input.TransferCommon.UV0 * TexCoordTransform_0.xy + TimeInSeconds * TexCoordTransform_0.zw;

    float4 color = float4(ColorEmissive.rgb, 1);

    color *= Texture_0.Sample(Sampler, uv);

    float3 cloudColor = GetCloudColor(Sampler, input.TransferCommon.CloudUV);
    color.rgb *= cloudColor;

    // TODO: Fog.

    return color;
}