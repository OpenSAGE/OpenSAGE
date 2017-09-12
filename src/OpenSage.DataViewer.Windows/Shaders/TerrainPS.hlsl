#include "Terrain.hlsli"

struct LightingConstants
{
    float3 CameraPosition;
    float3 AmbientLightColor;
    float3 Light0Direction;
    float3 Light0Color;
};

ConstantBuffer<LightingConstants> LightingCB : register(b0);

float4 main(PSInput input) : SV_TARGET
{
    float3 lightDir = LightingCB.Light0Direction;

    float3 diffuse = saturate(dot(input.WorldNormal, -lightDir))
        * float3(0.5, 0.5, 0.5);

    float3 ambient = LightingCB.AmbientLightColor;

    float3 totalObjectLighting = saturate(ambient + diffuse);

    float4 diffuseTextureColor = float4(1, 1, 1, 1);

    float3 color = (totalObjectLighting * diffuseTextureColor.rgb) * LightingCB.Light0Color;

    return float4(color, 1);
}