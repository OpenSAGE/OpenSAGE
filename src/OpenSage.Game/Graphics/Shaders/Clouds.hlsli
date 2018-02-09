#if !defined(CLOUD_TEXTURE_REGISTER)
#define CLOUD_TEXTURE_REGISTER t10
#endif

Texture2D<float4> Global_CloudTexture : register(CLOUD_TEXTURE_REGISTER);

float2 GetCloudUV(float3 worldPosition)
{
    // TODO: Wasteful to do a whole matrix-multiply here when we only need xy.
    float2 lightSpacePos = mul(float4(worldPosition, 1), CloudShadowMatrix).xy;

    float2 cloudTextureScale = float2(1 / 660.0, 1 / 660.0); // TODO: Read this from Weather.ini
    float2 offset = frac(TimeInSeconds * float2(-0.012, -0.018)); // TODO: Read this from Weather.ini

    return lightSpacePos * cloudTextureScale + offset;
}

float3 GetCloudColor(SamplerState samplerState, float2 cloudUV)
{
    return Global_CloudTexture.Sample(samplerState, cloudUV).rgb;
}