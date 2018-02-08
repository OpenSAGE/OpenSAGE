///////////////////////////////
// Buffers
///////////////////////////////

cbuffer GlobalConstantsShared : register(b0)
{
    float3 CameraPosition;
    float TimeInSeconds;
};

cbuffer GlobalConstantsVS : register(b1)
{
    row_major float4x4 ViewProjection;
};

#if !defined(GLOBAL_CONSTANTS_PS_REGISTER)
#define GLOBAL_CONSTANTS_PS_REGISTER b10
#endif

cbuffer GlobalConstantsPS : register(GLOBAL_CONSTANTS_PS_REGISTER)
{
    float2 ViewportSize;
};

static const float AlphaTestThreshold = 0x60 / (float) 0xFF;