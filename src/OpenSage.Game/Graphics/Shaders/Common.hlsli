///////////////////////////////
// Buffers
///////////////////////////////

cbuffer GlobalConstantsShared
{
    float3 CameraPosition;
};

cbuffer GlobalConstantsVS
{
    row_major float4x4 ViewProjection;
};

cbuffer GlobalConstantsPS
{
    float TimeInSeconds;
    float2 ViewportSize;
};

static const float AlphaTestThreshold = 0x60 / (float) 0xFF;