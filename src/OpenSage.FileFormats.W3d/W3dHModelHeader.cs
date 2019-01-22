using System.IO;
using System.Numerics;
using System.Collections.Generic;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHModelHeader : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_HMODEL_HEADER;

        public uint Version { get; private set; }

        public string Name { get; private set; }

        public string HierarchyName { get; private set; }

        public uint ConnectionsCount { get; private set; }

        public byte[] UnknownBytes { get; private set; }

        internal static W3dHModelHeader Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dHModelHeader
                {
                    Version = reader.ReadUInt32(),
                    Name = reader.ReadFixedLengthString(W3dConstants.NameLength),
                    HierarchyName = reader.ReadFixedLengthString(W3dConstants.NameLength),
                    ConnectionsCount = reader.ReadUInt16(),
                    UnknownBytes = reader.ReadBytes((int) context.CurrentEndPosition - (int) reader.BaseStream.Position)
                };

                // TODO: Determine W3dHModelHeader UnknownBytes

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
            writer.Write(HierarchyName);
            writer.Write(ConnectionsCount);
            writer.Write(UnknownBytes);
        }
    }
}
