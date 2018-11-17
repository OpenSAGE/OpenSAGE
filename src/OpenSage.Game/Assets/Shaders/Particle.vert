#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"

layout(set = 0, binding = 0) uniform GlobalConstantsShared
{
    GlobalConstantsSharedType _GlobalConstantsShared;
};

layout(set = 0, binding = 1) uniform GlobalConstantsVS
{
    GlobalConstantsVSType _GlobalConstantsVS;
};

layout(set = 0, binding = 2) uniform RenderItemConstants
{
    mat4 _World;
};

layout(location = 0) in vec3 in_Position;
layout(location = 1) in float in_Size;
layout(location = 2) in vec3 in_Color;
layout(location = 3) in float in_Alpha;
layout(location = 4) in float in_AngleZ;

layout(location = 0) out vec2 out_TexCoords;
layout(location = 1) out vec3 out_Color;
layout(location = 2) out float out_Alpha;

vec4 ComputePosition(vec3 particlePosition, float size, float angle, vec2 quadPosition)
{
    vec3 particlePosWS = (_World * vec4(particlePosition, 1)).xyz;

    vec3 toEye = normalize(_GlobalConstantsShared.CameraPosition - particlePosWS);
    vec3 up = vec3(cos(angle), 0, sin(angle));
    vec3 right = cross(toEye, up);
    up = cross(toEye, right);

    particlePosWS += (right * size * quadPosition.x) + (up * size * quadPosition.y);

    return _GlobalConstantsVS.ViewProjection * vec4(particlePosWS, 1);
}

void main()
{
    // Vertex layout:
    // 0 - 1
    // | / |
    // 2 - 3

    uint quadVertexID = uint(mod(gl_VertexIndex, 4));

    const vec4 vertexUVPos[4] = vec4[4]
    (
        vec4(0.0, 1.0, -1.0, -1.0),
        vec4(0.0, 0.0, -1.0, +1.0),
        vec4(1.0, 1.0, +1.0, -1.0),
        vec4(1.0, 0.0, +1.0, +1.0)
    );

    vec4 quadData = vertexUVPos[quadVertexID];

    gl_Position = ComputePosition(
        in_Position,
        in_Size,
        in_AngleZ,
        quadData.zw);

    out_TexCoords = quadData.xy;

    out_Color = in_Color;
    out_Alpha = in_Alpha;
}