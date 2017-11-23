#include "Common.hlsli"

struct VSOutput
{
    VSOutputCommon VSOutput;
    PSInputCommon TransferCommon;

};

struct SimpleConstants
{
    float3 ColorEmissive;
    float4 TexCoordTransform0;
};

ConstantBuffer<SimpleConstants> SimpleParamsCB : register(b0);

// ColorEmissive (float3)
// Texture_0
// TexCoordTransform_0 (float2? or float4?)
// DepthWriteEnable
// AlphaBlendingEnable
// FogEnable

VSOutput main(VSInputSkinnedInstanced input)
{
    VSOutput result;

    VSSkinnedInstanced(input, result.VSOutput, result.TransferCommon);

    return result;
}