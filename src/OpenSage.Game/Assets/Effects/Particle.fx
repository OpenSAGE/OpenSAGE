struct GlobalConstantsType
{
    float3 CameraPosition;
    float TimeInSeconds;

    matrix ViewProjection;
    float4 ClippingPlane1;
    float4 ClippingPlane2;
    bool HasClippingPlane1;
    bool HasClippingPlane2;
    
    float2 ViewportSize;
};

ConstantBuffer<GlobalConstantsType> GlobalConstants : register(space0);

float CalculateClippingPlane(float3 position, bool hasClippingPlane, float4 plane)
{
    if (hasClippingPlane)
    {
        return dot(float4(position, 1), plane);
    }
    return 1;
}

#define DO_CLIPPING(position) \
    output.ClipDistance0 = CalculateClippingPlane(position, GlobalConstants.HasClippingPlane1, GlobalConstants.ClippingPlane1); \
    output.ClipDistance1 = CalculateClippingPlane(position, GlobalConstants.HasClippingPlane2, GlobalConstants.ClippingPlane2)

struct RenderItemConstantsType
{
    matrix World;
};

ConstantBuffer<RenderItemConstantsType> RenderItemConstants : register(space1);

struct ParticleConstantsType
{
    float3 _Padding;
    bool IsGroundAligned;
};

ConstantBuffer<ParticleConstantsType> ParticleConstants : register(space1);

struct VertexInput
{
    float3 Position : POSITION;
    float Size      : TEXCOORD0;
    float3 Color    : TEXCOORD1;
    float Alpha     : TEXCOORD2;
    float AngleZ    : TEXCOORD3;

    uint VertexIndex : SV_VertexID;
};

struct VertexOutput
{
    float4 Position  : POSITION;

    float2 TexCoords : TEXCOORD0;
    float3 Color     : TEXCOORD1;
    float Alpha      : TEXCOORD2;

    float ClipDistance0 : SV_ClipDistance0;
    float ClipDistance1 : SV_ClipDistance1;
};

float4 ComputePosition(float3 particlePosition, float size, float angle, float2 quadPosition)
{
    float3 particlePosWS = (mul(RenderItemConstants.World, float4(particlePosition, 1))).xyz;

    float3 toEye;
    if (ParticleConstants.IsGroundAligned)
    {
        toEye = float3(0, 0, 1);
    }
    else
    {
        toEye = normalize(GlobalConstants.CameraPosition - particlePosWS);
    }

    float3 up = float3(cos(angle), 0, sin(angle));
    float3 right = cross(up, toEye);
    up = cross(toEye, right);

    particlePosWS += (right * size * quadPosition.x) + (up * size * quadPosition.y);

    return mul(GlobalConstants.ViewProjection, float4(particlePosWS, 1));
}

VertexOutput VS(VertexInput input)
{
    VertexOutput output;

    DO_CLIPPING(input.Position);

    // Vertex layout:
    // 0 - 1
    // | / |
    // 2 - 3

    uint quadVertexID = uint(input.VertexIndex % 4);

    static float4 vertexUVPos[4] =
    {
        float4(0.0, 0.0, -1.0, -1.0),
        float4(1.0, 0.0, -1.0, +1.0),
        float4(0.0, 1.0, +1.0, -1.0),
        float4(1.0, 1.0, +1.0, +1.0)
    };

    float4 quadData = vertexUVPos[quadVertexID];

    output.Position = ComputePosition(
        input.Position,
        input.Size,
        input.AngleZ,
        quadData.zw);

    output.TexCoords = quadData.xy;

    output.Color = input.Color;
    output.Alpha = input.Alpha;

    return output;
}

Texture2D ParticleTexture : register(space1);
SamplerState Sampler : register(space1);

struct PixelOutput
{
    float4 Color : SV_Target;
};

PixelOutput PS(VertexOutput input)
{
    float4 texColor = ParticleTexture.Sample(Sampler, input.TexCoords);

    texColor = float4(
        texColor.xyz * input.Color,
        texColor.w * input.Alpha);

    // TODO: Alpha test

    PixelOutput output;
    output.Color = texColor;

    return output;
}

technique ParticleAlphaBlend
{
    pass MyPass
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();

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
}

technique ParticleAdditiveBlend
{
    pass MyPass
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();

        BlendEnable[0] = true;
        SrcBlend = ONE;
        DestBlend = ONE;
        BlendOp = ADD;
        SrcBlendAlpha = ONE;
        DestBlendAlpha = ONE;
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
}