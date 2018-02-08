#if !defined(CLOUD_TEXTURE_REGISTER)
#define CLOUD_TEXTURE_REGISTER t10
#endif

Texture2D<float4> Global_CloudTexture : register(CLOUD_TEXTURE_REGISTER);

float2 GetCloudUV(float3 worldPosition)
{
    float2 lightSpacePos = mul(float4(worldPosition, 1), CloudShadowMatrix).xy;
    return lightSpacePos / 500 - TimeInSeconds * 0.03;
}

float3 GetCloudColor(SamplerState samplerState, float2 cloudUV)
{
    return Global_CloudTexture.Sample(samplerState, cloudUV).rgb;
}