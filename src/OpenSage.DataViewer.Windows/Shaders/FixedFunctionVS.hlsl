#include "FixedFunction.hlsli"

struct VSInputFixedFunction
{
    VSInputSkinnedInstanced Common;
    uint VertexID : SV_VertexID;
};

StructuredBuffer<uint> MaterialIndices : register(t1);

VSOutputFixedFunction main(VSInputFixedFunction input)
{
    VSOutputFixedFunction result = (VSOutputFixedFunction) 0;

    VSSkinnedInstanced(input.Common, result.VSOutput, result.TransferCommon);

    // TODO: Make sure that material index is constant for all vertices in a triangle.
    result.Transfer.MaterialIndex = MaterialIndices[input.VertexID];

    return result;
}