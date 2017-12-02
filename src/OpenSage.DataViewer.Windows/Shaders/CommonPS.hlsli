///////////////////////////////
// Buffers
///////////////////////////////

cbuffer GlobalConstantsPS : register(b0)
{
    float3 CameraPosition;
    float TimeInSeconds;
    float2 ViewportSize;
};

static const float AlphaTestThreshold = 0x60 / (float) 0xFF;