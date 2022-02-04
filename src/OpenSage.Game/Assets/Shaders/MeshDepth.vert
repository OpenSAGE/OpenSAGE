#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Mesh.h"

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
        _RenderItemConstantsVS.World);

    DO_CLIPPING(worldPosition)
}