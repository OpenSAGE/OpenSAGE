using System.IO;
using System.Collections.Generic;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dAggregateClassInfo : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_AGGREGATE_INFO;

        public uint OriginalClassID { get; private set; }

        public uint Flags { get; private set; }

        public byte[] UnknownBytes { get; private set; }

        internal static W3dAggregateClassInfo Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dAggregateClassInfo
                {
                    OriginalClassID = reader.ReadUInt32(),
                    Flags = reader.ReadUInt32(),
                    UnknownBytes = reader.ReadBytes((int) context.CurrentEndPosition - (int) reader.BaseStream.Position)
                };

                // TODO: Determine what the flags do/are.
                // TODO: Determine W3dAggregateClassInfo UnknownBytes.

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(OriginalClassID);
            writer.Write(Flags);
            writer.Write(UnknownBytes);
        }
    }
}
