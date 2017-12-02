#include "MeshCommonVS.hlsli"
#include "NormalMapped.hlsli"

VSOutputSimple main(VSInputSkinnedInstanced input)
{
    VSOutputSimple result;

    VSSkinnedInstanced(input, result.VSOutput, result.TransferCommon);

    return result;
}