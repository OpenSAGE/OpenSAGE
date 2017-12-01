///////////////////////////////
// Buffers
///////////////////////////////

cbuffer GlobalConstantsPS : register(b0)
{
    float3 CameraPosition;
    float TimeInSeconds;
    float2 ViewportSize;
};