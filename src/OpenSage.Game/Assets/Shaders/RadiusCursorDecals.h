#ifndef RADIUS_CURSOR_DECALS_H

#define RADIUS_CURSOR_DECALS_H

struct RadiusCursorDecal
{
    vec2 BottomLeftCornerPosition;
    float Diameter;
    uint DecalTextureIndex;
    vec3 _Padding;
    float Opacity;
};

#define MAKE_RADIUS_CURSOR_DECAL_RESOURCES(resourceSet) \
    layout(set = resourceSet, binding = 0) uniform texture2DArray RadiusCursorDecalTextures; \
    layout(set = resourceSet, binding = 1) uniform sampler RadiusCursorDecalSampler; \
    layout(set = resourceSet, binding = 2) uniform RadiusCursorDecalConstants \
    { \
        vec3 _Padding; \
        uint NumRadiusCursorDecals; \
    } _RadiusCursorDecalConstants; \
    layout(std430, set = resourceSet, binding = 3) readonly buffer RadiusCursorDecals \
    { \
        RadiusCursorDecal _RadiusCursorDecals[]; \
    };

#endif