#include "Simple.hlsli"

// ColorEmissive (float3)
// Texture_0
// TexCoordTransform_0 (float2? or float4?)
// DepthWriteEnable
// AlphaBlendingEnable
// FogEnable

VSOutputSimple main(VSInputSkinnedInstanced input)
{
    VSOutputSimple result;

    VSSkinnedInstanced(input, result.VSOutput, result.TransferCommon);

    return result;
}