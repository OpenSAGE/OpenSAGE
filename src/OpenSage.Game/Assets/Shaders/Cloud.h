#ifndef CLOUD_H

#define CLOUD_H

layout(set = PASS_CONSTANTS_RESOURCE_SET, binding = 2) uniform texture2D Global_CloudTexture;
layout(set = PASS_CONSTANTS_RESOURCE_SET, binding = 3) uniform sampler Global_CloudSampler;

vec2 GetCloudUV(vec3 worldPosition)
{
    // TODO: Wasteful to do a whole matrix-multiply here when we only need xy.
    vec2 lightSpacePos = (_GlobalLightingConstantsVS.CloudShadowMatrix * vec4(worldPosition, 1)).xy;

    vec2 cloudTextureScale = vec2(1 / 660.0f, 1 / 660.0f); // TODO: Read this from Weather.ini
    vec2 offset = fract(_GlobalConstants.TimeInSeconds * vec2(-0.012f, -0.018f)); // TODO: Read this from Weather.ini

    return (lightSpacePos * cloudTextureScale) + offset;
}

vec3 GetCloudColor(vec2 cloudUV)
{
    return texture(sampler2D(Global_CloudTexture, Global_CloudSampler), cloudUV).xyz;
}

#endif