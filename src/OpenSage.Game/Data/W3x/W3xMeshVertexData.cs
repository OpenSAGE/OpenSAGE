using System;
using System.IO;
using OpenSage.Data.StreamFS.AssetReaders;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3x
{
    public sealed class W3xMeshVertexData
    {
        public uint VertexCount { get; private set; }
        public uint VertexSize { get; private set; }
        public byte[] VertexData { get; private set; }
        public VertexDescription VertexDescription { get; private set; }
        public ushort[] BoneNumbers { get; private set; }

        internal static W3xMeshVertexData Parse(BinaryReader reader, AssetParseContext context)
        {
            var vertexCount = reader.ReadUInt32();
            var vertexSize = reader.ReadUInt32();

            var result = new W3xMeshVertexData
            {
                VertexCount = vertexCount,
                VertexSize = vertexSize,
                VertexData = reader.ReadAtOffset(() => reader.ReadBytes((int) (vertexCount * vertexSize)))
            };

            switch (context.Game)
            {
                case SageGame.Cnc3:
                case SageGame.Cnc3KanesWrath:
                    result.VertexDescription = D3d9VertexDescription.Parse(reader);
                    break;

                case SageGame.Ra3:
                case SageGame.Ra3Uprising:
                case SageGame.Cnc4:
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
