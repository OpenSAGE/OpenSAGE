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
            public Vector3 HouseColor;
            public /* bool */ uint HasHouseColor;
            public /* bool */ uint SkinningEnabled;
            public uint NumBones;
        }

        public struct RenderItemConstantsVS
        {
            public Matrix4x4 World;
        }
        
        public static void GetSkinnedVertexData(
            ref VertexInput input,
            Matrix4x4 skinning)
        {
            input.Position = Vector3.Transform(input.Position, skinning);
            input.Normal = TransformNormal(input.Normal, skinning);
        }

        public static void VSSkinnedInstancedPositionOnly(
            VertexInput input,
            out Vector4 position,
            out Vector3 worldPosition,
            Matrix4x4 world,
            Matrix4x4 viewProjection)
        {
            var worldPositionHomogeneous = Vector4.Transform(input.Position, world);

            position = Vector4.Transform(worldPositionHomogeneous, viewProjection);

            worldPosition = worldPositionHomogeneous.XYZ();
        }

        public static void VSSkinnedInstanced(
            VertexInput input,
            out Vector4 position,
            out Vector3 worldPosition,
            out Vector3 worldNormal,
            out Vector2 cloudUV,
            Matrix4x4 world,
            Matrix4x4 viewProjection,
            Matrix4x4 cloudShadowMatrix,
            float timeInSeconds)
        {
            VSSkinnedInstancedPositionOnly(
                input,
                out position,
                out worldPosition,
                world,
                viewProjection);

            worldNormal = TransformNormal(input.Normal, world);

            cloudUV = CloudHelpers.GetCloudUV(
                worldPosition,
                cloudShadowMatrix,
                timeInSeconds);
        }
    }
}
