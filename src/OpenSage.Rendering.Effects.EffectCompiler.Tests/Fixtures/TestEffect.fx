struct GlobalConstantsSharedType
{
    float3 CameraPosition;
    float TimeInSeconds;
};

struct GlobalConstantsPSType
{
    float2 ViewportSize;
};

ConstantBuffer<GlobalConstantsSharedType> GlobalConstantsShared : register(space0);
ConstantBuffer<GlobalConstantsPSType> GlobalConstantsPS : register(space0);

//cbuffer Globals2 : register(space0)
//{
//    GlobalConstantsPSType GlobalConstantsPS;
//};

Texture2D Texture : register(space1);
SamplerState Sampler : register(space1);

struct VSInput
{
    float3 Position : POSITION;
    float Size : TEXCOORD0;
    float3 Color    : TEXCOORD1;
    float Alpha : TEXCOORD2;
    float AngleZ : TEXCOORD3;
};

struct PSInput
{
    float2 TexCoords : TEXCOORD0;
    float3 Color     : TEXCOORD1;
    float Alpha : TEXCOORD2;
};

PSInput VS(VSInput input)
{
    PSInput output;

    output.Color = GlobalConstantsShared.CameraPosition;

    return output;
}

float4 PS(PSInput input) : SV_Target
{
    float4 texColor = Texture.Sample(Sampler, input.TexCoords);

    texColor = float4(
        texColor.xyz * input.Color,
        texColor.w * input.Alpha);

    texColor.x += GlobalConstantsPS.ViewportSize.x;

    texColor.xyz += GlobalConstantsShared.CameraPosition;

    return texColor;
}

technique MyTechnique
{
    pass MyPass
    {
        VertexShader = compile vs_6_0 VS();
        PixelShader = compile ps_6_0 PS();

        BlendEnable[0] = true;
        SrcBlend = SRC_ALPHA;
        DestBlend = INV_SRC_ALPHA;
        BlendOp = ADD;
        SrcBlendAlpha = SRC_ALPHA;
        DestBlendAlpha = INV_SRC_ALPHA;
        BlendOpAlpha = ADD;

        ZEnable = true;
        ZWriteEnable = false;
        ZFunc = LESSEQUAL;

        CullMode = BACK;
        FillMode = SOLID;
        FrontCounterClockwise = true;
        DepthClipEnable = true;
        ScissorEnable = false;
    }

    pass MyPass2
    {
        VertexShader = compile vs_6_0 VS();
        PixelShader = compile ps_6_0 PS();
    }
}

technique MyTechnique2
{
    pass MyPass
    {
        VertexShader = compile vs_6_0 VS();
        PixelShader = compile ps_6_0 PS();
    }
}