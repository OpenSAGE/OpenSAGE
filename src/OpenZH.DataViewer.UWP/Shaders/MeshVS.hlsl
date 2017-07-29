#include "Mesh.hlsli"

struct VSInput
{
	float3 Position  : POSITION;
  float3 Normal    : NORMAL;
  float2 UV        : TEXCOORD0;
};

struct MeshTransformConstants
{
  float4x4 WorldViewProjection;
  float4x4 World;
};

ConstantBuffer<MeshTransformConstants> MeshTransformCB : register(b0);

PSInput main(VSInput input)
{
	PSInput result;

	result.Position = mul(float4(input.Position, 1), MeshTransformCB.WorldViewProjection);
  result.WorldPosition = mul(input.Position, (float3x3) MeshTransformCB.World);
 
  result.Normal = mul(input.Normal, (float3x3) MeshTransformCB.World);
  result.UV = input.UV;

	return result;
}