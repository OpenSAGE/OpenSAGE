#define MESH_VERTEX_INPUTS \
    layout(location = 0) in vec3 in_Position; \
    layout(location = 1) in vec3 in_Normal; \
    layout(location = 2) in vec3 in_Tangent; \
    layout(location = 3) in vec3 in_Binormal; \
    layout(location = 4) in uint in_BoneIndex; \
    layout(location = 5) in vec2 in_UV0; \
    layout(location = 6) in vec2 in_UV1;

struct MeshConstantsType
{
  vec3 HouseColor;
  bool HasHouseColor;
  bool SkinningEnabled;
  uint NumBones;
  vec2 _Padding;
};

struct RenderItemConstantsVSType
{
    mat4 World;
};

void GetSkinnedVertexData(
    vec3 inputPosition,
    vec3 inputNormal,
    mat4 skinning,
    out vec3 modifiedPosition,
    out vec3 modifiedNormal)
{
    modifiedPosition = (skinning * vec4(inputPosition, 1)).xyz;
    modifiedNormal = TransformNormal(inputNormal, skinning);
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