using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;

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

        public static readonly VertexDescriptor VertexDescriptor = new VertexDescriptor(
            new[]
            {
                new VertexAttributeDescription(InputClassification.PerVertexData, "POSITION", 0, VertexFormat.Float3, 0, 0),
                new VertexAttributeDescription(InputClassification.PerVertexData, "NORMAL", 0, VertexFormat.Float3, 12, 0),
                new VertexAttributeDescription(InputClassification.PerVertexData, "TANGENT", 0, VertexFormat.Float3, 24, 0),
                new VertexAttributeDescription(InputClassification.PerVertexData, "BINORMAL", 0, VertexFormat.Float3, 36, 0),
                new VertexAttributeDescription(InputClassification.PerVertexData, "BLENDINDICES", 0, VertexFormat.UInt, 48, 0),

                new VertexAttributeDescription(InputClassification.PerVertexData, "TEXCOORD", 0, VertexFormat.Float2, 0, 1),
                new VertexAttributeDescription(InputClassification.PerVertexData, "TEXCOORD", 1, VertexFormat.Float2, 8, 1),

                new VertexAttributeDescription(InputClassification.PerInstanceData, "TEXCOORD", 2, VertexFormat.Float4, 0, 2),
                new VertexAttributeDescription(InputClassification.PerInstanceData, "TEXCOORD", 3, VertexFormat.Float4, 16, 2),
                new VertexAttributeDescription(InputClassification.PerInstanceData, "TEXCOORD", 4, VertexFormat.Float4, 32, 2),
                new VertexAttributeDescription(InputClassification.PerInstanceData, "TEXCOORD", 5, VertexFormat.Float4, 48, 2),
            },
            new[]
            {
                new VertexLayoutDescription(52),
                new VertexLayoutDescription(16),
                new VertexLayoutDescription(64)
            });
    }
}
