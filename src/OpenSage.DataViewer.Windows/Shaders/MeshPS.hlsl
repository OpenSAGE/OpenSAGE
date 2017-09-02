#include "Mesh.hlsli"

struct LightingConstants
{
    float3 CameraPosition;
    float3 AmbientLightColor;
    float3 Light0Direction;
    float3 Light0Color;
};

ConstantBuffer<LightingConstants> LightingCB : register(b0);

struct VertexMaterial
{
    float3 Ambient;
    float3 Diffuse;
    float3 Specular;
    float Shininess;
    float3 Emissive;
    float Opacity;
};

StructuredBuffer<VertexMaterial> Materials : register(t0);

Buffer<uint> TextureIndices : register(t1);
Texture2D<float4> Textures[] : register(t2);

SamplerState Sampler : register(s0);

float4 main(PSInput input) : SV_TARGET
{
    VertexMaterial material = Materials[input.MaterialIndex];

    float3 lightDir = LightingCB.Light0Direction;

    float diffuseLighting = saturate(dot(input.Normal, -lightDir));

    float3 h = normalize(normalize(LightingCB.CameraPosition - input.WorldPosition) - lightDir);
  
    //float specularLighting = pow(saturate(dot(h, input.Normal)), material.Shininess);
    float specularLighting = 0;

    uint textureIndex = TextureIndices[input.PrimitiveID];
    Texture2D<float4> diffuseTexture = Textures[NonUniformResourceIndex(textureIndex)];
    float4 diffuseTexel = diffuseTexture.Sample(Sampler, input.UV);

    float3 ambientColor = LightingCB.AmbientLightColor * material.Ambient;
    float3 diffuseColor = diffuseTexel.xyz * material.Diffuse * LightingCB.Light0Color * diffuseLighting;
    float3 specularColor = material.Specular * specularLighting;

    float3 totalColor = diffuseColor;
    //float3 totalColor = ambientColor + diffuseColor + specularColor;

    return float4(
        saturate(totalColor),
        diffuseTexel.w);
}