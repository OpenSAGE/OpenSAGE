struct GlobalConstantsSharedType
{
    vec3 CameraPosition;
    float TimeInSeconds;
};

struct GlobalConstantsVSType
{
    mat4 ViewProjection;
};

struct GlobalConstantsPSType
{
    vec2 ViewportSize;
};

bool FailsAlphaTest(float alpha)
{
    // 0x60 / 0xFF = 0.37647
    return alpha < 0.37647f;
}

vec3 TransformNormal(vec3 v, mat4 m)
{
    return (m * vec4(v, 0)).xyz;
}

float saturate(float v)
{
    return clamp(v, 0.0, 1.0);
}

vec3 saturate(vec3 v)
{
    return clamp(v, vec3(0, 0, 0), vec3(1, 1, 1));
}