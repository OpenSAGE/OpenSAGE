#version 450

layout(set = 0, binding = 1) uniform SpriteConstants
{
    vec3 _Padding;
    bool IgnoreAlpha;
} _SpriteConstants;

layout(set = 1, binding = 0) uniform sampler Sampler;

layout(set = 2, binding = 0) uniform texture2D Texture;

layout(location = 0) in vec2 in_UV;
layout(location = 1) in vec4 in_Color;

layout(location = 0) out vec4 out_Color;

void main()
{
    vec4 textureColor = texture(sampler2D(Texture, Sampler), in_UV);

    if (_SpriteConstants.IgnoreAlpha)
    {
        textureColor.w = 1;
    }

    textureColor *= in_Color;

    out_Color = textureColor;
}