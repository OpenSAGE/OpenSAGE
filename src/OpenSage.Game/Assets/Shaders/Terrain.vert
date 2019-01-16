#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Lighting.h"
#include "Mesh.h"

MAKE_GLOBAL_CONSTANTS_RESOURCES_VS(0)

MAKE_GLOBAL_LIGHTING_CONSTANTS_RESOURCES_VS(1)

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
    out_WorldPosition = in_Position;

    gl_Position = _GlobalConstantsVS.ViewProjection * vec4(out_WorldPosition, 1);

    out_WorldNormal = in_Normal;

    out_UV = in_UV;

    out_CloudUV = GetCloudUV(
        out_WorldPosition,
        _GlobalLightingConstantsVS.CloudShadowMatrix,
        _GlobalConstantsShared.TimeInSeconds);

    out_ViewSpaceDepth = gl_Position.z;
}