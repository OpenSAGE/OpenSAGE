#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "ForwardPass.h"

layout(location = 0) in vec3 in_Position;

layout(location = 0) out vec3 out_WorldPosition;
layout(location = 1) out vec2 out_CloudUV;
layout(location = 2) out float out_ViewSpaceDepth;

void main()
{
    vec3 worldPosition = in_Position;

    gl_Position = _GlobalConstants.ViewProjection * vec4(worldPosition, 1);
    out_WorldPosition = worldPosition;

    out_CloudUV = GetCloudUV(out_WorldPosition);

    out_ViewSpaceDepth = gl_Position.z;
}