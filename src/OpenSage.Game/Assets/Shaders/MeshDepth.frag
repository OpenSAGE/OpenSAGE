#version 450
#extension GL_GOOGLE_include_directive : enable

#include "Common.h"
#include "Mesh.h"

MAKE_GLOBAL_CONSTANTS_RESOURCES_PS(0)

MAKE_MESH_CONSTANTS_RESOURCES(1)

layout(set = 2, binding = 1) uniform RenderItemConstantsPS
{
    RenderItemConstantsPSType _RenderItemConstantsPS;
};

void main()
{
    // TODO: Alpha testing
}