struct PSInput
{
    float4 Position  : SV_POSITION;
    float2 TexCoords : TEXCOORD;
};

PSInput VS(float2 position : POSITION, float2 uv : TEXCOORD)
{
    PSInput output;

    output.Position = float4(position, 0, 1);
    output.TexCoords = uv;

    return output;
}

Texture2D<float4> Texture : register(t0);
SamplerState Sampler : register(s0);

cbuffer MaterialConstants : register(b2)
{
    uint MipMapLevel;
};

float4 PS(PSInput input) : SV_TARGET
{
    return Texture.SampleLevel(
        Sampler,
        input.TexCoords,
        MipMapLevel);
}