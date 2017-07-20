#include <metal_stdlib>
#include <simd/simd.h>

using namespace metal;

struct WVP
{
    float4x4 WorldViewProjection;
    float4x4 World;
};

struct Lighting
{
    packed_float3 CameraPosition;
    packed_float3 AmbientLightColor;
	packed_float3 Light0Direction;
	packed_float3 Light0Color;
};

struct Material
{
	packed_float3 Ambient;
	packed_float3 Diffuse;
	packed_float3 Specular;
	packed_float3 Emissive;
	float Shininess;
	float Opacity;
};

struct VSInput
{
    packed_float3 Position [[attribute(0)]];
    packed_float3 Normal   [[attribute(1)]];
	packed_float2 UV       [[attribute(2)]];
};
 
struct PSInput
{
    float4 Position [[position]];
    float3 WorldPosition;
    float3 Normal;
	float2 UV;
};

vertex PSInput VSMain(
	VSInput input     [[stage_in]],
    constant WVP& wvp [[buffer(0)]])
{
    PSInput result;
    
    result.Position = wvp.WorldViewProjection * float4(input.Position, 1);
    result.WorldPosition = input.Position * (float3x3) wvp.World;

	result.Normal = input.Normal * (float3x3) wvp.World;
	result.UV = input.UV;
    
    return result;
}

fragment half4 PSMain(
	PSInput input               [[stage_in]],
	constant Lighting& lighting [[buffer(0)]],
	constant Material& material [[buffer(1)]])
{
	float3 lightDir = _lighting.Light0Direction;
 
	float diffuseLighting = saturate(dot(input.Normal, -lightDir));
 
	float3 h = normalize(normalize(lighting.CameraPosition - input.WorldPosition) - lightDir);
	//float specLighting = pow(saturate(dot(h, input.Normal)), material.Shininess);
	float specLighting = 0;
	float4 texel = float4(1, 1, 1, 1); // tex2D(texsampler, input.TexCoords);
 
	return float4(
		saturate(
		  lighting.AmbientLightColor +
		  (texel.xyz * material.Diffuse * lighting.Light0Color * diffuseLighting) +
		  (material.Specular * specLighting)
		), 
		texel.w);
}
