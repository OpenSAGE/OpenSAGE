#include "MeshCommonPS.hlsli"
#include "NormalMapped.hlsli"

#define SPECULAR_ENABLED
#define LIGHTING_TYPE Object
#include "Lighting.hlsli"

cbuffer MaterialConstants : register(b2)
{
    float BumpScale;
    float SpecularExponent;
    bool AlphaTestEnable;
    float4 DiffuseColor;
    float4 SpecularColor;
};

Texture2D<float4> DiffuseTexture : register(t0);
Texture2D<float4> NormalMap : register(t1);

SamplerState Sampler : register(s0);

float4 main(VSOutputSimple input) : SV_Target
{
    LightingParameters lightingParams;
    lightingParams.WorldPosition = input.TransferCommon.WorldPosition;
    lightingParams.WorldNormal = input.TransferCommon.WorldNormal;
    lightingParams.MaterialAmbient = float3(0, 0, 0);
    lightingParams.MaterialDiffuse = DiffuseColor.rgb;
    lightingParams.MaterialSpecular = SpecularColor.rgb;
    lightingParams.MaterialShininess = SpecularExponent;

    float3 diffuseColor;
    float3 specularColor;
    DoLighting(lightingParams, diffuseColor, specularColor);

    float2 uv = input.TransferCommon.UV0;
    float4 diffuseTextureColor = DiffuseTexture.Sample(Sampler, uv);

    if (AlphaTestEnable)
    {
        if (diffuseTextureColor.a < AlphaTestThreshold)
        {
            discard;
        }
    }

    float3 objectColor = diffuseTextureColor.rgb * diffuseColor;

    objectColor += specularColor;

    float4 normal = NormalMap.Sample(Sampler, uv); // TODO
    objectColor += normal.rgb * 0.00001;

    return float4(
        objectColor,
        DiffuseColor.a * diffuseTextureColor.a);
}