struct VSOutputSimple
{
    VSOutputCommon VSOutput;
    PSInputCommon TransferCommon;

    float3 WorldTangent : TANGENT;
    float3 WorldBinormal : BINORMAL;
};