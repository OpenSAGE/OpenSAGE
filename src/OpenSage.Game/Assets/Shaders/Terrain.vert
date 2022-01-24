#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "ForwardPass.h"

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec3 in_Normal;
layout(location = 2) in vec2 in_UV;

layout(location = 0) out vec3 out_WorldPosition;
layout(location = 1) out vec3 out_WorldNormal;
layout(location = 2) out vec2 out_UV;
layout(location = 3) out vec2 out_CloudUV;
layout(location = 4) out float out_ViewSpaceDepth;
layout(location = 5) out vec4 out_ClippingPlane;

void main()
{
    DO_CLIPPING(in_Position)
    out_ClippingPlane = _GlobalConstants.ClippingPlane1;

    out_WorldPosition = in_Position;

    gl_Position = _GlobalConstants.ViewProjection * vec4(out_WorldPosition, 1);

    out_WorldNormal = in_Normal;

    out_UV = in_UV;

    out_CloudUV = GetCloudUV(out_WorldPosition);

    out_ViewSpaceDepth = gl_Position.z;
}