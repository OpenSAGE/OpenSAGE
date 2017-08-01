#include "Mesh.hlsli"

struct LightingConstants
{
  float3 CameraPosition;
  float3 AmbientLightColor;
  float3 Light0Direction;
  float3 Light0Color;
};

struct MaterialConstants
{
  float3 MaterialAmbient;
  float3 MaterialDiffuse;
  float3 MaterialSpecular;
  float MaterialShininess;
  float3 MaterialEmissive;
  float MaterialOpacity;
};

ConstantBuffer<LightingConstants> LightingCB : register(b0);
ConstantBuffer<MaterialConstants> MaterialCB : register(b1);

Buffer<uint> TextureIndices : register(t0);

Texture2D<float4> Textures[] : register(t1);

SamplerState Sampler : register(s0);

float4 main(PSInput input) : SV_TARGET
{
  float3 lightDir = LightingCB.Light0Direction;
 
  float diffuseLighting = saturate(dot(input.Normal, -lightDir));
 
  float3 h = normalize(normalize(LightingCB.CameraPosition - input.WorldPosition) - lightDir);
  //float specLighting = pow(saturate(dot(h, input.Normal)), MaterialShininess);
  float specLighting = 0;

  uint textureIndex = TextureIndices[input.PrimitiveID];
  float4 texel = Textures[textureIndex].Sample(Sampler, input.UV);
 
  return float4(
    saturate(
      LightingCB.AmbientLightColor +
      (texel.xyz * MaterialCB.MaterialDiffuse * LightingCB.Light0Color * diffuseLighting) +
      (MaterialCB.MaterialSpecular * specLighting)
    ), 
    texel.w);
}