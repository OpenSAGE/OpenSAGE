#include "Common.hlsli"

struct TransferFixedFunction
{
    uint MaterialIndex : TEXCOORD4;
};

struct VSOutputFixedFunction
{
    PSInputCommon TransferCommon;
    TransferFixedFunction Transfer;
    VSOutputCommon VSOutput;
};

struct PSInputFixedFunction
{
    PSInputCommon TransferCommon;
    TransferFixedFunction Transfer;

    uint PrimitiveID : SV_PrimitiveID;
    float4 ScreenPosition : SV_Position;
};