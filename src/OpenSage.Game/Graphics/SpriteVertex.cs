using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SpriteVertex
    {
        public Vector3 Position;
        public Vector2 UV;
        public ColorRgbaF Color;

        public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
            new VertexElementDescription("POSITION", VertexElementSemantic.Position, VertexElementFormat.Float3),
            new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("COLOR", VertexElementSemantic.Color, VertexElementFormat.Float4));
    }
}
