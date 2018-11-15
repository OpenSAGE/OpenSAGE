#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Lighting.h"
#include "Cloud.h"

layout(set = 0, binding = 0) uniform GlobalConstantsSharedUniform
{
    GlobalConstantsSharedType GlobalConstantsShared;
};

layout(set = 0, binding = 1) uniform GlobalConstantsVSUniform
{
    GlobalConstantsVSType GlobalConstantsVS;
};

layout(set = 0, binding = 2) uniform GlobalLightingConstantsVSUniform
{
    GlobalLightingConstantsVSType GlobalLightingConstantsVS;
};

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec3 in_Normal;
layout(location = 2) in vec2 in_UV;

layout(location = 0) out vec3 out_WorldPosition;
layout(location = 1) out vec3 out_WorldNormal;
layout(location = 2) out vec2 out_UV;
layout(location = 3) out vec2 out_CloudUV;

void main()
{
    vec3 worldPosition = in_Position;

    gl_Position = GlobalConstantsVS.ViewProjection * vec4(worldPosition, 1);
    out_WorldPosition = worldPosition;

    out_WorldNormal = in_Normal;

    out_UV = in_UV;

    out_CloudUV = GetCloudUV(
        out_WorldPosition,
        GlobalLightingConstantsVS.CloudShadowMatrix,
        GlobalConstantsShared.TimeInSeconds);
}