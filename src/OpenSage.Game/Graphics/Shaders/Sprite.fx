struct VSInput
{
    float3 Position : POSITION;
    float2 UV : TEXCOORD;
    float4 Color : COLOR;
};

struct PSInput
{
    float4 Position  : SV_POSITION;
    float2 TexCoords : TEXCOORD;
    float4 Color     : COLOR;
};

cbuffer MaterialConstantsVS
{
    row_major matrix Projection;
};

PSInput VS(VSInput input)
{
    PSInput output;

    output.Position = mul(float4(input.Position, 1), Projection);
    output.TexCoords = input.UV;
    output.Color = input.Color;

    return output;
}

Texture2D<float4> Texture : register(t0);
SamplerState Sampler : register(s0);

float4 PS(PSInput input) : SV_TARGET
{
    float4 textureColor = Texture.SampleLevel(
        Sampler,
        input.TexCoords,
        0);

    textureColor *= input.Color;

    return textureColor;
}