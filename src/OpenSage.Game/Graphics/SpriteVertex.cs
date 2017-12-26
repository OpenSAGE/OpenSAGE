using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Graphics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SpriteVertex
    {
        public Vector2 Position;
        public Vector2 UV;

        public SpriteVertex(Vector2 position, Vector2 uv)
        {
            Position = position;
            UV = uv;
        }

        public static readonly VertexDescriptor VertexDescriptor = new VertexDescriptor(
            new[]
            {
                new VertexAttributeDescription("POSITION", 0, VertexFormat.Float2, 0, 0),
                new VertexAttributeDescription("TEXCOORD", 0, VertexFormat.Float2, 8, 0)
            },
            new[]
            {
                new VertexLayoutDescription(InputClassification.PerVertexData, 16)
            });
    }
}
