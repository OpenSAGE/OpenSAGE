#include "MeshCommonVS.hlsli"
#include "FixedFunction.hlsli"

VSOutputFixedFunction main(VSInputSkinnedInstanced input)
{
    VSOutputFixedFunction result = (VSOutputFixedFunction) 0;

    VSSkinnedInstanced(input, result.VSOutput, result.TransferCommon);

    return result;
}