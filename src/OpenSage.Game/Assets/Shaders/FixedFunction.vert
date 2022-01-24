#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "ForwardPass.h"
#include "Mesh.h"

MESH_VERTEX_INPUTS

layout(location = 0) out vec3 out_WorldPosition;
layout(location = 1) out vec3 out_WorldNormal;
layout(location = 2) out vec2 out_UV0;
layout(location = 3) out vec2 out_UV1;
layout(location = 4) out vec2 out_CloudUV;
layout(location = 5) out float out_ViewSpaceDepth;

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

    VSSkinnedInstanced(
        modifiedPosition,
        modifiedNormal,
        gl_Position,
        out_WorldPosition,
        out_WorldNormal,
        out_CloudUV,
        _RenderItemConstantsVS.World);

    DO_CLIPPING(out_WorldPosition)

    out_UV0 = in_UV0;
    out_UV1 = in_UV1;

    out_ViewSpaceDepth = gl_Position.z;
}