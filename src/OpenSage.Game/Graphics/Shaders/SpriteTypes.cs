using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal static class SpriteTypes
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MaterialConstantsVS
        {
            public Matrix4x4 Projection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpriteConstantsPS
        {
            private readonly Vector3 _padding;
            public Bool32 IgnoreAlpha;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpriteVertex
        {
            public Vector3 Position;
            public Vector2 UV;
            public ColorRgbaF Color;

            public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
                new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("COLOR", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));
        }
    }
}
