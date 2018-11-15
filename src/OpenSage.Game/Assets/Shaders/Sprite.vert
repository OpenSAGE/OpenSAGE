#version 450

layout(set = 0, binding = 0) uniform ProjectionUniform
{
    mat4 Projection;
};

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec2 in_UV;
layout(location = 2) in vec4 in_Color;

layout(location = 0) out vec2 out_UV;
layout(location = 1) out vec4 out_Color;

void main()
{
    gl_Position = Projection * vec4(in_Position, 1);

    out_UV = in_UV;
    out_Color = in_Color;
}