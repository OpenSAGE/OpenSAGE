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
layout(location = 1) in vec3 in_Normal;
layout(location = 2) in vec2 in_UV;

layout(location = 0) out vec3 out_WorldPosition;
layout(location = 1) out vec3 out_WorldNormal;
layout(location = 2) out vec2 out_UV;
layout(location = 3) out vec2 out_CloudUV;
layout(location = 4) out float out_ViewSpaceDepth;

void main()
{
    vec3 worldPosition = in_Position;

    gl_Position = _GlobalConstantsVS.ViewProjection * vec4(worldPosition, 1);
    out_WorldPosition = worldPosition;

    out_WorldNormal = in_Normal;

    out_UV = in_UV;

    out_CloudUV = GetCloudUV(
        out_WorldPosition,
        _GlobalLightingConstantsVS.CloudShadowMatrix,
        _GlobalConstantsShared.TimeInSeconds);

    out_ViewSpaceDepth = gl_Position.z;
}