#include "Common.hlsli"

struct PSInput
{
    float4 Position  : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
    float3 Color     : TEXCOORD1;
    float  Alpha : TEXCOORD2;
};

struct VSInput
{
    float3 Position : POSITION;
    float  Size : TEXCOORD0;
    float3 Color    : TEXCOORD1;
    float  Alpha : TEXCOORD2;
    float  AngleZ : TEXCOORD3;

    uint VertexID : SV_VertexID;
};

cbuffer RenderItemConstantsVS : register(b1)
{
    row_major matrix World;
};

static const float4 VertexUVPos[4] =
{
    { 0.0, 1.0, -1.0, -1.0 },
    { 0.0, 0.0, -1.0, +1.0 },
    { 1.0, 1.0, +1.0, -1.0 },
    { 1.0, 0.0, +1.0, +1.0 }
};

float4 ComputePosition(float3 particlePosition, float size, float angle, float2 quadPosition)
{
    float3 particlePosWS = mul(float4(particlePosition, 1), World).xyz;

    float3 toEye = normalize(CameraPosition - particlePosWS);
    float3 up = { cos(angle), 0, sin(angle) };
    float3 right = cross(toEye, up);
    up = cross(toEye, right);

    particlePosWS += (right * size * quadPosition.x) + (up * size * quadPosition.y);

    return mul(float4(particlePosWS, 1), ViewProjection);
}

PSInput VS(VSInput input)
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

Texture2D<float4> ParticleTexture : register(t0);
SamplerState LinearSampler : register(s0);

float4 PS(PSInput input) : SV_TARGET
{
    float4 texColor = ParticleTexture.Sample(
        LinearSampler,
        input.TexCoords);

    texColor.rgb *= input.Color;

    texColor.a *= input.Alpha;

    return texColor;

    // TODO: Alpha test
}