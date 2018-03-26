using System.Numerics;
using ShaderGen;
using static OpenSage.Graphics.Shaders.CommonShaderHelpers;

namespace OpenSage.Graphics.Shaders
{
    public static class MeshShaderHelpers
    {
        public struct VertexInput
        {
            [PositionSemantic] public Vector3 Position;

            [NormalSemantic] public Vector3 Normal;
            [TextureCoordinateSemantic] public Vector3 Tangent;
            [TextureCoordinateSemantic] public Vector3 Binormal;

            [TextureCoordinateSemantic] public uint BoneIndex;

            [TextureCoordinateSemantic] public Vector2 UV0;
            [TextureCoordinateSemantic] public Vector2 UV1;
        }

        public struct MeshConstants
        {
            public /* bool */ uint SkinningEnabled;
            public uint NumBones;
        }

        public struct RenderItemConstantsVS
        {
            public Matrix4x4 World;
        }

        public static void VSSkinnedInstanced(
            VertexInput input,
            out Vector4 position,
            out Vector3 worldPosition,
            out Vector3 worldNormal,
            out Vector2 cloudUV,
            StructuredBuffer<Matrix4x4> skinningBuffer,
            uint skinningEnabled,
            uint numBones,
            Matrix4x4 world,
            Matrix4x4 viewProjection,
            Matrix4x4 cloudShadowMatrix,
            float timeInSeconds)
        {
            if (skinningEnabled == 1)
            {
                var skinning = skinningBuffer[input.BoneIndex];

                input.Position = Vector3.Transform(input.Position, skinning);
                input.Normal = TransformNormal(input.Normal, skinning);
            }

            var worldPositionHomogeneous = Vector4.Transform(input.Position, world);

            position = Vector4.Transform(worldPositionHomogeneous, viewProjection);

            worldPosition = worldPositionHomogeneous.XYZ();

            worldNormal = TransformNormal(input.Normal, world);

            cloudUV = CloudHelpers.GetCloudUV(
                worldPosition,
                cloudShadowMatrix,
                timeInSeconds);
        }
    }
}
