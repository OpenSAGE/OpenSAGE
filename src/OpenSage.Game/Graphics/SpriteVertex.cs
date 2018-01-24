using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Graphics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SpriteVertex
    {
        public Vector3 Position;
        public Vector2 UV;
        public ColorRgbaF Color;

        public static readonly VertexDescriptor VertexDescriptor = new VertexDescriptor(
            new[]
            {
                new VertexAttributeDescription("POSITION", 0, VertexFormat.Float3, 0, 0),
                new VertexAttributeDescription("TEXCOORD", 0, VertexFormat.Float2, 12, 0),
                new VertexAttributeDescription("COLOR", 0, VertexFormat.Float4, 20, 0)
            },
            new[]
            {
                new VertexLayoutDescription(InputClassification.PerVertexData, 32)
            });
    }
}
