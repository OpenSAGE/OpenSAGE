using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace OpenSage.Graphics
{
    public static class MeshVertex
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Basic
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 Tangent;
            public Vector3 Binormal;
            public uint BoneIndex;
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
                new VertexElementDescription("NORMAL", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TANGENT", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("BINORMAL", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("BLENDINDICES", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1)),

            new VertexLayoutDescription(
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
        };
    }
}
