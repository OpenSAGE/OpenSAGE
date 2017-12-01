struct VSOutputFixedFunction
{
    PSInputCommon TransferCommon;
    VSOutputCommon VSOutput;
};

struct PSInputFixedFunction
{
    PSInputCommon TransferCommon;

    float4 ScreenPosition : SV_Position;
};