#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"

MAKE_GLOBAL_CONSTANTS_RESOURCES_VS(0)

layout(set = 1, binding = 0) uniform RenderItemConstants
{
    mat4 _World;
};

layout(set = 1, binding = 1) uniform ParticleConstants
{
    vec3 _Padding;
    bool _IsGroundAligned;
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

    vec3 toEye;
    if (_IsGroundAligned)
    {
        toEye = vec3(0, 0, 1);
    }
    else
    {
        toEye = normalize(_GlobalConstants.CameraPosition - particlePosWS);
    }

    vec3 up = vec3(cos(angle), 0, sin(angle));
    vec3 right = cross(up, toEye);
    up = cross(toEye, right);

    particlePosWS += (right * size * quadPosition.x) + (up * size * quadPosition.y);

    return _GlobalConstants.ViewProjection * vec4(particlePosWS, 1);
}

void main()
{
    DO_CLIPPING(in_Position)

    // Vertex layout:
    // 0 - 1
    // | / |
    // 2 - 3

    uint quadVertexID = uint(mod(gl_VertexIndex, 4));

    const vec4 vertexUVPos[4] = vec4[4]
    (
        vec4(0.0, 0.0, -1.0, -1.0),
        vec4(1.0, 0.0, -1.0, +1.0),
        vec4(0.0, 1.0, +1.0, -1.0),
        vec4(1.0, 1.0, +1.0, +1.0)
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