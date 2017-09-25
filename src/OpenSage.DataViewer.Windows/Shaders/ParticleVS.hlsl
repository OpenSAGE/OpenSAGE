#include "Particle.hlsli"

struct VSInput
{
    float3 Position : POSITION;
    float  Size     : TEXCOORD0;
    float3 Color    : TEXCOORD1;
    float  Alpha    : TEXCOORD2;
    float  AngleZ   : TEXCOORD3;

    uint VertexID : SV_VertexID;
};

struct ParticleTransformConstants
{
    row_major matrix WorldViewProjection;
    float3 CameraPosition;
};

ConstantBuffer<ParticleTransformConstants> TransformCB : register(b0);

static const float4 VertexUVPos[4] =
{
    { 0.0, 1.0, -1.0, -1.0 },
    { 0.0, 0.0, -1.0, +1.0 },
    { 1.0, 1.0, +1.0, -1.0 },
    { 1.0, 0.0, +1.0, +1.0 },
};

float4 ComputePosition(float3 particlePosition, float size, float angle, float2 quadPosition)
{
    float3 toEye = normalize(TransformCB.CameraPosition - particlePosition);
    float3 up    = { cos(angle), 0, sin(angle) };
    float3 right = cross(toEye, up);
    up = cross(toEye, right);

    particlePosition += (right * size * quadPosition.x) + (up * size * quadPosition.y);

    return mul(float4(particlePosition, 1), TransformCB.WorldViewProjection);
}

PSInput main(VSInput input)
{
    PSInput output;

    // Vertex layout:
    // 0 - 1
    // | / |
    // 2 - 3

    float quadVertexID = input.VertexID % 4;

    output.Position = ComputePosition(
        input.Position, 
        input.Size,
        input.AngleZ,
        VertexUVPos[quadVertexID].zw);

    output.TexCoords = VertexUVPos[quadVertexID].xy;

    output.Color = input.Color;
    output.Alpha = input.Alpha;
    
    return output;
}