#define MESH_VERTEX_INPUTS \
    layout(location = 0) in vec3 in_Position0; \
    layout(location = 1) in vec3 in_Position1; \
    layout(location = 2) in vec3 in_Normal; \
    layout(location = 3) in vec3 in_Tangent; \
    layout(location = 4) in vec3 in_Binormal; \
    layout(location = 5) in uint in_BoneIndex0; \
    layout(location = 6) in uint in_BoneIndex1; \
    layout(location = 7) in float in_BoneWeight0; \
    layout(location = 8) in float in_BoneWeight1; \
    layout(location = 9) in vec2 in_UV0; \
    layout(location = 10) in vec2 in_UV1;

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
    float _Padding;
};

void GetSkinnedVertexData(
    vec3 inputPosition0,
    vec3 inputPosition1,
    vec3 inputNormal,
    mat4 skinning0,
    mat4 skinning1,
    float weight0,
    float weight1,
    out vec3 modifiedPosition,
    out vec3 modifiedNormal)
{
    modifiedPosition = (skinning0 * vec4(inputPosition0, 1)).xyz * weight0;
    modifiedPosition += (skinning1 * vec4(inputPosition1, 1)).xyz * weight1;

    modifiedNormal = TransformNormal(inputNormal, skinning0);
}

void VSSkinnedInstancedPositionOnly(
    vec3 inputPosition,
    out vec4 position,
    out vec3 worldPosition,
    mat4 world,
    mat4 viewProjection)
{
    vec4 worldPositionHomogeneous = world * vec4(inputPosition, 1);

    position = viewProjection * worldPositionHomogeneous;

    worldPosition = worldPositionHomogeneous.xyz;
}

void VSSkinnedInstanced(
    vec3 inputPosition,
    vec3 inputNormal,
    out vec4 position,
    out vec3 worldPosition,
    out vec3 worldNormal,
    out vec2 cloudUV,
    mat4 world,
    mat4 viewProjection,
    mat4 cloudShadowMatrix,
    float timeInSeconds)
{
    VSSkinnedInstancedPositionOnly(
        inputPosition,
        position,
        worldPosition,
        world,
        viewProjection);

    worldNormal = TransformNormal(inputNormal, world);

    cloudUV = GetCloudUV(
        worldPosition,
        cloudShadowMatrix,
        timeInSeconds);
}