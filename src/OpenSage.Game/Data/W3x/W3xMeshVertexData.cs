using System;
using System.IO;
using OpenSage.Data.StreamFS;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.W3x
{
    public sealed class W3xMeshVertexData
    {
        public uint VertexCount { get; private set; }
        public uint VertexSize { get; private set; }
        public byte[] VertexData { get; private set; }
        public VertexDescription VertexDescription { get; private set; }
        public ushort[] BoneNumbers { get; private set; }

        internal static W3xMeshVertexData Parse(BinaryReader reader, AssetEntry header)
        {
            var vertexCount = reader.ReadUInt32();
            var vertexSize = reader.ReadUInt32();

            var result = new W3xMeshVertexData
            {
                VertexCount = vertexCount,
                VertexSize = vertexSize,
                VertexData = reader.ReadAtOffset(() => reader.ReadBytes((int) (vertexCount * vertexSize)))
            };

            switch (header.TypeHash)
            {
                case 3386369912u: // Cnc3
                case 1750982594u: // Cnc3KanesWrath - not sure what changed
                    result.VertexDescription = D3d9VertexDescription.Parse(reader);
                    break;

                case 3448375452u: // Ra3 and above
                    result.VertexDescription = RnaVertexDescription.Parse(reader);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            result.BoneNumbers = reader.ReadArrayAtOffset(() => reader.ReadUInt16());

            return result;
        }
    }
}
