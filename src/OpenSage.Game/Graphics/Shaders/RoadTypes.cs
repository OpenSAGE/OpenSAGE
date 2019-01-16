using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal static class RoadTypes
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RoadVertex
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
