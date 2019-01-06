#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Cloud.h"
#include "Mesh.h"

layout(set = 0, binding = 0) uniform GlobalConstantsVS
{
    GlobalConstantsVSType _GlobalConstantsVS;
};

layout(set = 0, binding = 1) uniform MeshConstants
{
    MeshConstantsType _MeshConstants;
};

layout(set = 0, binding = 2) uniform RenderItemConstantsVS
{
    RenderItemConstantsVSType _RenderItemConstantsVS;
};

layout(set = 0, binding = 3) readonly buffer SkinningBuffer
{
    mat4 _SkinningBuffer[];
};

MESH_VERTEX_INPUTS

void main()
{
    vec3 modifiedPosition = in_Position0;
    vec3 modifiedNormal = in_Normal0;

    if (_MeshConstants.SkinningEnabled)
    {
        GetSkinnedVertexData(
            in_Position0,
            in_Position1,
            in_Normal0,
            in_Normal1,
            _SkinningBuffer[in_BoneIndex0],
            _SkinningBuffer[in_BoneIndex1],
            in_BoneWeight0,
            in_BoneWeight1,
            modifiedPosition,
            modifiedNormal);
    }

    vec3 worldPosition;
    VSSkinnedInstancedPositionOnly(
        modifiedPosition,
        gl_Position,
        worldPosition,
        _RenderItemConstantsVS.World,
        _GlobalConstantsVS.ViewProjection);
}