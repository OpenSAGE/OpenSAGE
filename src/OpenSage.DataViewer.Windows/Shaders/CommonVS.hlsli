///////////////////////////////
// Buffers
///////////////////////////////

cbuffer GlobalConstantsVS : register(b0)
{
    row_major float4x4 ViewProjection;
    float3 CameraPosition;
};