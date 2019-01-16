using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal static class MeshTypes
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MeshConstants
        {
#pragma warning disable CS0169
            private readonly Vector2 _padding;
#pragma warning restore CS0169
            public Bool32 SkinningEnabled;
            public Bool32 HasHouseColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RenderItemConstantsVS
        {
            public Matrix4x4 World;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RenderItemConstantsPS
        {
            public Vector3 HouseColor;
#pragma warning disable CS0169
            private readonly float _padding;
#pragma warning restore CS0169
        }

        public static class MeshVertex
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Basic
            {
                public Vector3 Position0;
                public Vector3 Position1;
                public Vector3 Normal0;
                public Vector3 Normal1;
                public Vector3 Tangent;
                public Vector3 Binormal;
                public uint BoneIndex0;
                public uint BoneIndex1;
                public float BoneWeight0;
                public float BoneWeight1;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct TexCoords
            {
                public Vector2 UV0;
                public Vector2 UV1;
            }

            public static readonly VertexLayoutDescription[] VertexDescriptors = new[]
            {
            new VertexLayoutDescription(
                new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("NORMAL", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("NORMAL", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TANGENT", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("BINORMAL", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("BLENDINDICES", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1),
                new VertexElementDescription("BLENDINDICES", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1),
                new VertexElementDescription("BLENDINDICES", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
                new VertexElementDescription("BLENDINDICES", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1)),

            new VertexLayoutDescription(
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
        };
        }
    }
}
