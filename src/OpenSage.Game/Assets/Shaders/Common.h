#ifndef COMMON_H

#define COMMON_H

struct GlobalConstantsSharedType
{
    vec3 CameraPosition;
    float TimeInSeconds;
};

struct GlobalConstantsVSType
{
    mat4 ViewProjection;
    vec4 ClippingPlane;
};

struct GlobalConstantsPSType
{
    vec2 ViewportSize;
};

#define MAKE_GLOBAL_CONSTANTS_RESOURCES_VS(resourceSet) \
    layout(set = resourceSet, binding = 0) uniform GlobalConstantsShared \
    { \
        GlobalConstantsSharedType _GlobalConstantsShared; \
    }; \
    \
    layout(set = resourceSet, binding = 1) uniform GlobalConstantsVS \
    { \
        GlobalConstantsVSType _GlobalConstantsVS; \
    };

#define MAKE_GLOBAL_CONSTANTS_RESOURCES_PS(resourceSet) \
    layout(set = resourceSet, binding = 0) uniform GlobalConstantsShared \
    { \
        GlobalConstantsSharedType _GlobalConstantsShared; \
    }; \
    \
    layout(set = resourceSet, binding = 2) uniform GlobalConstantsPS \
    { \
        GlobalConstantsPSType _GlobalConstantsPS; \
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

float CalculateClippingPlane(vec3 position, vec4 plane)
{
    if (plane.x != 0 || plane.y != 0 || plane.z != 0 || plane.w != 0)
        return dot(vec4(position, 1), plane);
    return 1;
}
#endif