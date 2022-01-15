#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Lighting.h"
#include "Mesh.h"

MAKE_MESH_RESOURCES_VS()

MESH_VERTEX_INPUTS

layout(location = 0) out vec3 out_WorldPosition;
layout(location = 1) out vec3 out_WorldNormal;
layout(location = 2) out vec2 out_UV0;
layout(location = 3) out vec2 out_CloudUV;
layout(location = 4) out vec3 out_WorldTangent;
layout(location = 5) out vec3 out_WorldBinormal;

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
        _RenderItemConstantsVS.World,
        _GlobalConstants.ViewProjection,
        _GlobalLightingConstantsVS.CloudShadowMatrix,
        _GlobalConstants.TimeInSeconds);
        
    DO_CLIPPING(out_WorldPosition)

    out_UV0 = in_UV0;

    out_WorldTangent = TransformNormal(in_Tangent, _RenderItemConstantsVS.World);
    out_WorldBinormal = TransformNormal(in_Binormal, _RenderItemConstantsVS.World);
}