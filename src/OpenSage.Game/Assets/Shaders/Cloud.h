#ifndef CLOUD_H

#define CLOUD_H

#define MAKE_GLOBAL_CLOUD_RESOURCES_PS(resourceSet) \
    layout(set = resourceSet, binding = 0) uniform texture2D Global_CloudTexture;

vec2 GetCloudUV(
    vec3 worldPosition,
    mat4 cloudShadowMatrix,
    float timeInSeconds)
{
    // TODO: Wasteful to do a whole matrix-multiply here when we only need xy.
    vec2 lightSpacePos = (cloudShadowMatrix * vec4(worldPosition, 1)).xy;
    
    vec2 cloudTextureScale = vec2(1 / 660.0f, 1 / 660.0f); // TODO: Read this from Weather.ini
    vec2 offset = fract(timeInSeconds * vec2(-0.012f, -0.018f)); // TODO: Read this from Weather.ini

    return (lightSpacePos * cloudTextureScale) + offset;
}

vec3 GetCloudColor(
    texture2D cloudTexture,
    sampler cloudSampler,
    vec2 cloudUV)
{
    return texture(sampler2D(cloudTexture, cloudSampler), cloudUV).xyz;
}

#endif