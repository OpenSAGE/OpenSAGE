using System.IO;

namespace OpenSage.FileFormats.W3d { 

    public sealed class W3dAggregateHeader : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_AGGREGATE_HEADER;

        public uint Version { get; private set; }

        public string Name { get; private set; }

        internal static W3dAggregateHeader Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dAggregateHeader
                {
                    Version = reader.ReadUInt32(),
                    Name = reader.ReadFixedLengthString(W3dConstants.NameLength)
                };

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
        }
    }
}
