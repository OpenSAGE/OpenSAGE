struct VSInput
{
	float3 Position : POSITION;
  float3 Normal : NORMAL;
  float2 TexCoords : TEXCOORD0;
};

struct PSInput
{
	float4 Position : SV_POSITION;
  float3 WorldPosition : TEXCOORD0;
  float3 Normal : TEXCOORD1;
  float2 TexCoords : TEXCOORD2;
};

struct WVP
{
	row_major float4x4 WorldViewProjection;
  row_major float4x4 World;
};

struct Lighting
{
  float3 CameraPosition;
  float3 AmbientLightColor;
  float3 Light0Direction;
  float3 Light0Color;
};

struct Material
{
  float3 MaterialAmbient;
  float3 MaterialDiffuse;
  float3 MaterialSpecular;
  float3 MaterialEmissive;
  float MaterialShininess;
  float MaterialOpacity;
};

ConstantBuffer<WVP> _wvp : register(b0);

ConstantBuffer<Lighting> _lighting : register(b1);

ConstantBuffer<Material> _material : register(b2);

PSInput VSMain(VSInput input)
{
	PSInput result;

	result.Position = mul(float4(input.Position, 1), _wvp.WorldViewProjection);
  result.WorldPosition = mul(input.Position, (float3x3) _wvp.World);
 
  result.Normal = mul(input.Normal, (float3x3) _wvp.World);
  result.TexCoords = input.TexCoords;

	return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
  float3 lightDir = _lighting.Light0Direction;
 
  float diffuseLighting = saturate(dot(input.Normal, -lightDir));
 
  float3 h = normalize(normalize(_lighting.CameraPosition - input.WorldPosition) - lightDir);
  //float specLighting = pow(saturate(dot(h, input.Normal)), MaterialShininess);
  float specLighting = 0;
  float4 texel = float4(1, 1, 1, 1); // tex2D(texsampler, input.TexCoords);
 
  return float4(
    saturate(
      _lighting.AmbientLightColor +
      (texel.xyz * _material.MaterialDiffuse * _lighting.Light0Color * diffuseLighting) +
      (_material.MaterialSpecular * specLighting)
    ), 
    texel.w);
}