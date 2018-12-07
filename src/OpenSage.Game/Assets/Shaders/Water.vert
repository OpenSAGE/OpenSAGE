#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Lighting.h"
#include "Cloud.h"

layout(set = 0, binding = 0) uniform GlobalConstantsShared
{
    GlobalConstantsSharedType _GlobalConstantsShared;
};

layout(set = 0, binding = 1) uniform GlobalConstantsVS
{
    GlobalConstantsVSType _GlobalConstantsVS;
};

layout(set = 0, binding = 2) uniform GlobalLightingConstantsVS
{
    GlobalLightingConstantsVSType _GlobalLightingConstantsVS;
};

layout(location = 0) in vec3 in_Position;

layout(location = 0) out vec3 out_WorldPosition;
layout(location = 3) out vec2 out_CloudUV;
layout(location = 4) out float out_ViewSpaceDepth;

void main()
{
    vec3 worldPosition = in_Position;

    gl_Position = _GlobalConstantsVS.ViewProjection * vec4(worldPosition, 1);
    out_WorldPosition = worldPosition;

    out_CloudUV = GetCloudUV(
        out_WorldPosition,
        _GlobalLightingConstantsVS.CloudShadowMatrix,
        _GlobalConstantsShared.TimeInSeconds);

    out_ViewSpaceDepth = gl_Position.z;
}