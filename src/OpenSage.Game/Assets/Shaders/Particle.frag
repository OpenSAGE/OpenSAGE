#version 450

layout(set = 0, binding = 3) uniform texture2D ParticleTexture;
layout(set = 0, binding = 4) uniform sampler Sampler;

layout(location = 0) in vec2 in_TexCoords;
layout(location = 1) in vec3 in_Color;
layout(location = 2) in float in_Alpha;

layout(location = 0) out vec4 out_Color;

void main()
{
    vec4 texColor = texture(sampler2D(ParticleTexture, Sampler), in_TexCoords);

    texColor = vec4(
        texColor.xyz * in_Color,
        texColor.w * in_Alpha);

    // TODO: Alpha test

    out_Color = texColor;
}