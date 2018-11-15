#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Lighting.h"
#include "Cloud.h"
#include "Mesh.h"

layout(set = 0, binding = 0) uniform GlobalConstantsSharedUniform
{
    GlobalConstantsSharedType GlobalConstantsShared;
};

layout(set = 0, binding = 1) uniform GlobalConstantsVSUniform
{
    GlobalConstantsVSType GlobalConstantsVS;
};

layout(set = 0, binding = 2) uniform GlobalLightingConstantsVSUniform
{
    GlobalLightingConstantsVSType GlobalLightingConstantsVS;
};

layout(set = 0, binding = 3) uniform MeshConstantsUniform
{
    MeshConstantsType MeshConstants;
};

layout(set = 0, binding = 4) uniform RenderItemConstantsVSUniform
{
    RenderItemConstantsVSType RenderItemConstantsVS;
};

layout(set = 0, binding = 5) readonly buffer SkinningBufferBlock
{
    mat4 SkinningBuffer[];
};

MESH_VERTEX_INPUTS

layout(location = 0) out vec3 out_WorldPosition;
layout(location = 1) out vec3 out_WorldNormal;
layout(location = 2) out vec2 out_UV0;
layout(location = 3) out vec2 out_UV1;
layout(location = 4) out vec2 out_CloudUV;

void main()
{
    vec3 modifiedPosition = in_Position;
    vec3 modifiedNormal = in_Normal;

    if (MeshConstants.SkinningEnabled)
    {
        GetSkinnedVertexData(
            in_Position, 
            in_Normal,
            SkinningBuffer[in_BoneIndex],
            modifiedPosition,
            modifiedNormal);
    }

    VSSkinnedInstanced(
        in_Position,
        in_Normal,
        gl_Position,
        out_WorldPosition,
        out_WorldNormal,
        out_CloudUV,
        RenderItemConstantsVS.World,
        GlobalConstantsVS.ViewProjection,
        GlobalLightingConstantsVS.CloudShadowMatrix,
        GlobalConstantsShared.TimeInSeconds);

    out_UV0 = in_UV0;
    out_UV1 = in_UV1;
}