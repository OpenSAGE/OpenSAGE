using System;

namespace OpenSage.Data.W3d
{
    [Flags]
    public enum W3dVertexMaterialFlags : uint
    {
        None = 0,

        UseDepthCue = 0x00000001,
        ArgbEmissiveOnly = 0x00000002,
        CopySpecularToDiffuse = 0x00000004,
        DepthCueToAlpha = 0x00000008,

        Stage0MappingMask =0x00FF0000,
        Stage0MappingUv = 0x00000000,
        Stage0MappingEnvironment = 0x00010000,
        Stage0MappingCheapEnvironment = 0x00020000,
        Stage0MappingScreen = 0x00030000,
        Stage0MappingLinearOffset = 0x00040000,
        Stage0MappingSilhouette = 0x00050000,
        Stage0MappingScale = 0x00060000,
        Stage0MappingGrid = 0x00070000,
        Stage0MappingRotate = 0x00080000,
        Stage0MappingSineLinearOffset = 0x00090000,
        Stage0MappingStepLinearOffset = 0x000A0000,
        Stage0MappingZigZagLinearOffset = 0x000B0000,
        Stage0MappingWsClassicEnv = 0x000C0000,
        Stage0MappingWsEnvironment = 0x000D0000,
        Stage0MappingGridClassicEnv = 0x000E0000,
        Stage0MappingGridEnvironment = 0x000F0000,
        Stage0MappingRandom = 0x00100000,
        Stage0MappingEdge = 0x00110000,
        Stage0MappingBumpEnv = 0x00120000,

        Stage1MappingMask = 0x0000FF00,
        Stage1MappingUv = 0x00000000,
        Stage1MappingEnvironment = 0x00000100,
        Stage1MappingCheapEnvironment = 0x00000200,
        Stage1MappingScreen = 0x00000300,
        Stage1MappingLinearOffset = 0x00000400,
        Stage1MappingSilhouette = 0x00000500,
        Stage1MappingScale = 0x00000600,
        Stage1MappingGrid = 0x00000700,
        Stage1MappingRotate = 0x00000800,
        Stage1MappingSineLinearOffset = 0x00000900,
        Stage1MappingStepLinearOffset = 0x00000A00,
        Stage1MappingZigZagLinearOffset = 0x00000B00,
        Stage1MappingWsClassicEnv = 0x00000C00,
        Stage1MappingWsEnvironment = 0x00000D00,
        Stage1MappingGridClassicEnv = 0x00000E00,
        Stage1MappingGridEnvironment = 0x00000F00,
        Stage1MappingRandom = 0x00001000,
        Stage1MappingEdge = 0x00001100,
        Stage1MappingBumpEnv = 0x00001200,

        PsxMask = 0xFF000000,
        PsxTransMask = 0x07000000,
        PsxTransNone = 0x00000000,
        PsxTrans100 = 0x01000000,
        PsxTrans50 = 0x02000000,
        PsxTrans25 = 0x03000000,
        PsxTransMinus100 = 0x04000000,
        PsxNoRtLighting = 0x08000000
    }
}
