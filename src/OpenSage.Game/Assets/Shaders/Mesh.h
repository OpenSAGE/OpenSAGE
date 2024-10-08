#ifndef MESH_H

#define MESH_H

#define MESH_VERTEX_INPUTS \
    layout(location = 0) in vec3 in_Position0; \
    layout(location = 1) in vec3 in_Position1; \
    layout(location = 2) in vec3 in_Normal0; \
    layout(location = 3) in vec3 in_Normal1; \
    layout(location = 4) in vec3 in_Tangent; \
    layout(location = 5) in vec3 in_Binormal; \
    layout(location = 6) in uint in_BoneIndex0; \
    layout(location = 7) in uint in_BoneIndex1; \
    layout(location = 8) in float in_BoneWeight0; \
    layout(location = 9) in float in_BoneWeight1; \
    layout(location = 10) in vec2 in_UV0; \
    layout(location = 11) in vec2 in_UV1;

struct MeshConstantsType
{
    vec2 _Padding;
    bool SkinningEnabled;
    bool HasHouseColor;
};

struct RenderItemConstantsVSType
{
    mat4 World;
};

struct RenderItemConstantsPSType
{
    vec3 HouseColor;
    float Opacity;
    vec3 TintColor;
    float _Padding;
};

layout(set = RENDER_ITEM_CONSTANTS_RESOURCE_SET, binding = 0) uniform MeshConstants
{
    MeshConstantsType _MeshConstants;
};

layout(set = RENDER_ITEM_CONSTANTS_RESOURCE_SET, binding = 1) uniform RenderItemConstantsVS
{
    RenderItemConstantsVSType _RenderItemConstantsVS;
};

layout(set = RENDER_ITEM_CONSTANTS_RESOURCE_SET, binding = 2) readonly buffer SkinningBuffer
{
    mat4 _SkinningBuffer[];
};

layout(set = RENDER_ITEM_CONSTANTS_RESOURCE_SET, binding = 3) uniform RenderItemConstantsPS
{
    RenderItemConstantsPSType _RenderItemConstantsPS;
};

void GetSkinnedVertexData(
    vec3 inputPosition0,
    vec3 inputPosition1,
    vec3 inputNormal0,
    vec3 inputNormal1,
    mat4 skinning0,
    mat4 skinning1,
    float weight0,
    float weight1,
    out vec3 modifiedPosition,
    out vec3 modifiedNormal)
{
    modifiedPosition = (skinning0 * vec4(inputPosition0, 1)).xyz * weight0;
    modifiedPosition += (skinning1 * vec4(inputPosition1, 1)).xyz * weight1;

    modifiedNormal = TransformNormal(inputNormal0, skinning0) * weight0;
    modifiedNormal += TransformNormal(inputNormal1, skinning1) * weight1;
}

void VSSkinnedInstancedPositionOnly(
    vec3 inputPosition,
    out vec4 position,
    out vec3 worldPosition,
    mat4 world)
{
    vec4 worldPositionHomogeneous = world * vec4(inputPosition, 1);

    position = _GlobalConstants.ViewProjection * worldPositionHomogeneous;

    worldPosition = worldPositionHomogeneous.xyz;
}

#if FORWARD_PASS

void VSSkinnedInstanced(
    vec3 inputPosition,
    vec3 inputNormal,
    out vec4 position,
    out vec3 worldPosition,
    out vec3 worldNormal,
    out vec2 cloudUV,
    mat4 world)
{
    VSSkinnedInstancedPositionOnly(
        inputPosition,
        position,
        worldPosition,
        world);

    worldNormal = TransformNormal(inputNormal, world);

    cloudUV = GetCloudUV(worldPosition);
}

#endif

#endif