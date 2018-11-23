using System;
using System.IO;
using OpenSage.Data.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Data.W3x
{
    public abstract class VertexDescription
    {
        public abstract VertexElementDescription[] GetVertexElementDescriptions();
    }

    public sealed class RnaVertexDescription : VertexDescription
    {
        public string VertexDeclaration { get; private set; }

        internal static RnaVertexDescription Parse(BinaryReader reader)
        {
            return new RnaVertexDescription
            {
                VertexDeclaration = reader.ReadUInt32PrefixedAsciiStringAtOffset()
            };
        }

        public override VertexElementDescription[] GetVertexElementDescriptions()
        {
            // For example:
            // p0:00:3f32 n0:0C:3f32 c0:18:4u8n g0:1C:3f32 b0:28:3f32 t0:34:2f32

            var elements = VertexDeclaration.Split(' ');
            var result = new VertexElementDescription[elements.Length];
            for (var i = 0; i < result.Length; i++)
            {
                var parts = elements[i].Split(':');
                result[i] = new VertexElementDescription(
                    parts[0],
                    VertexElementSemantic.TextureCoordinate,
                    GetFormat(parts[2]),
                    Convert.ToUInt32(parts[1], 16));
            }
            return result;
        }

        private static VertexElementFormat GetFormat(string format)
        {
            switch (format)
            {
                case "2f32":
                    return VertexElementFormat.Float2;

                case "3f32":
                    return VertexElementFormat.Float3;

                case "4u8n":
                    return VertexElementFormat.Byte4_Norm;

                default:
                    throw new ArgumentOutOfRangeException(nameof(format));
            }
        }
    }

    public sealed class D3d9VertexDescription : VertexDescription
    {
        public D3d9VertexElement[] VertexElements { get; private set; }

        internal static D3d9VertexDescription Parse(BinaryReader reader)
        {
            var vertexElementSize = reader.ReadUInt32();
            var vertexElementCount = vertexElementSize / 8; // sizeof(D3DVERTEXELEMENT9) == 8
            return new D3d9VertexDescription
            {
                VertexElements = reader.ReadFixedSizeArrayAtOffset(
                    () => D3d9VertexElement.Parse(reader),
                    vertexElementCount)
            };
        }

        public override VertexElementDescription[] GetVertexElementDescriptions()
        {
            // Ignore last VertexElement, because it's D3DDECLEND
            var result = new VertexElementDescription[VertexElements.Length - 1];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = VertexElements[i].GetVertexElementDescription();
            }
            return result;
        }
    }

    public sealed class D3d9VertexElement
    {
        public ushort Stream { get; private set; }
        public ushort Offset { get; private set; }
        public D3DDECLTYPE Type { get; private set; }
        public D3DDECLMETHOD Method { get; private set; }
        public D3DDECLUSAGE Usage { get; private set; }
        public byte UsageIndex { get; private set; }

        internal static D3d9VertexElement Parse(BinaryReader reader)
        {
            return new D3d9VertexElement
            {
                Stream = reader.ReadUInt16(),
                Offset = reader.ReadUInt16(),
                Type = reader.ReadByteAsEnum<D3DDECLTYPE>(),
                Method = reader.ReadByteAsEnum<D3DDECLMETHOD>(),
                Usage = reader.ReadByteAsEnum<D3DDECLUSAGE>(),
                UsageIndex = reader.ReadByte()
            };
        }

        public VertexElementDescription GetVertexElementDescription()
        {
            return new VertexElementDescription
            {
                Name = $"{Usage}{UsageIndex}",
                Offset = Offset,
                Semantic = VertexElementSemantic.TextureCoordinate,
                Format = ToVertexElementFormat(Type)
            };
        }

        private static VertexElementFormat ToVertexElementFormat(D3DDECLTYPE type)
        {
            switch (type)
            {
                case D3DDECLTYPE.D3DDECLTYPE_FLOAT1:
                    return VertexElementFormat.Float1;
                case D3DDECLTYPE.D3DDECLTYPE_FLOAT2:
                    return VertexElementFormat.Float2;
                case D3DDECLTYPE.D3DDECLTYPE_FLOAT3:
                    return VertexElementFormat.Float3;
                case D3DDECLTYPE.D3DDECLTYPE_FLOAT4:
                    return VertexElementFormat.Float4;
                case D3DDECLTYPE.D3DDECLTYPE_D3DCOLOR:
                    return VertexElementFormat.Byte4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }

    public enum D3DDECLTYPE : byte
    {
        D3DDECLTYPE_FLOAT1 = 0,
        D3DDECLTYPE_FLOAT2 = 1,
        D3DDECLTYPE_FLOAT3 = 2,
        D3DDECLTYPE_FLOAT4 = 3,
        D3DDECLTYPE_D3DCOLOR = 4,
        D3DDECLTYPE_UBYTE4 = 5,
        D3DDECLTYPE_SHORT2 = 6,
        D3DDECLTYPE_SHORT4 = 7,
        D3DDECLTYPE_UBYTE4N = 8,
        D3DDECLTYPE_SHORT2N = 9,
        D3DDECLTYPE_SHORT4N = 10,
        D3DDECLTYPE_USHORT2N = 11,
        D3DDECLTYPE_USHORT4N = 12,
        D3DDECLTYPE_UDEC3 = 13,
        D3DDECLTYPE_DEC3N = 14,
        D3DDECLTYPE_FLOAT16_2 = 15,
        D3DDECLTYPE_FLOAT16_4 = 16,
        D3DDECLTYPE_UNUSED = 17
    }

    public enum D3DDECLMETHOD : byte
    {
        D3DDECLMETHOD_DEFAULT = 0,
        D3DDECLMETHOD_PARTIALU = 1,
        D3DDECLMETHOD_PARTIALV = 2,
        D3DDECLMETHOD_CROSSUV = 3,
        D3DDECLMETHOD_UV = 4,
        D3DDECLMETHOD_LOOKUP = 5,
        D3DDECLMETHOD_LOOKUPPRESAMPLED = 6
    }

    public enum D3DDECLUSAGE : byte
    {
        D3DDECLUSAGE_POSITION = 0,
        D3DDECLUSAGE_BLENDWEIGHT = 1,
        D3DDECLUSAGE_BLENDINDICES = 2,
        D3DDECLUSAGE_NORMAL = 3,
        D3DDECLUSAGE_PSIZE = 4,
        D3DDECLUSAGE_TEXCOORD = 5,
        D3DDECLUSAGE_TANGENT = 6,
        D3DDECLUSAGE_BINORMAL = 7,
        D3DDECLUSAGE_TESSFACTOR = 8,
        D3DDECLUSAGE_POSITIONT = 9,
        D3DDECLUSAGE_COLOR = 10,
        D3DDECLUSAGE_FOG = 11,
        D3DDECLUSAGE_DEPTH = 12,
        D3DDECLUSAGE_SAMPLE = 13
    }
}
