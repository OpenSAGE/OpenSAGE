using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal static class TerrainTypes
    {
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        public struct TerrainMaterialConstants
        {
            [FieldOffset(0)]
            public Vector2 MapBorderWidth;

            [FieldOffset(8)]
            public Vector2 MapSize;

            [FieldOffset(16)]
            public bool IsMacroTextureStretched;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TerrainVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 UV;

            public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
                new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("NORMAL", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
        }
    }
}
