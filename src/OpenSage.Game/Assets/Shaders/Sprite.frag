#version 450

layout(set = 0, binding = 1) uniform texture2D Texture;
layout(set = 0, binding = 2) uniform sampler Sampler;

layout(location = 0) in vec2 in_UV;
layout(location = 1) in vec4 in_Color;

layout(location = 0) out vec4 out_Color;

void main()
{
    vec4 textureColor = texture(sampler2D(Texture, Sampler), in_UV);

    textureColor *= in_Color;

    out_Color = textureColor;
}