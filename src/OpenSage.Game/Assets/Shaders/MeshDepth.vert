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
    vec3 modifiedPosition = in_Position;
    vec3 modifiedNormal = in_Normal;

    if (_MeshConstants.SkinningEnabled)
    {
        GetSkinnedVertexData(
            in_Position, 
            in_Normal,
            _SkinningBuffer[in_BoneIndex],
            modifiedPosition,
            modifiedNormal);
    }

    vec3 worldPosition;
    VSSkinnedInstancedPositionOnly(
        in_Position,
        gl_Position,
        worldPosition,
        _RenderItemConstantsVS.World,
        _GlobalConstantsVS.ViewProjection);
}