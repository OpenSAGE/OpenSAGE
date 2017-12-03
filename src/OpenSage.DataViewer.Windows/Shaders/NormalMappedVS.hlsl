#include "MeshCommonVS.hlsli"
#include "NormalMapped.hlsli"

VSOutputSimple main(VSInputSkinnedInstanced input)
{
    VSOutputSimple result;

    VSSkinnedInstanced(input, result.VSOutput, result.TransferCommon);

    // TODO: Duplicated from MeshCommonVS.hlsli
    matrix world;
    world[0] = input.World0;
    world[1] = input.World1;
    world[2] = input.World2;
    world[3] = input.World3;

    result.WorldTangent = mul(input.Tangent, (float3x3) world);
    result.WorldBinormal = mul(input.Binormal, (float3x3) world);

    return result;
}