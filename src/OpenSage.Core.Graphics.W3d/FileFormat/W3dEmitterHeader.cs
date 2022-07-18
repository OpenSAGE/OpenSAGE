using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dEmitterHeader : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_EMITTER_HEADER;

        public uint Version { get; private set; }

        public string Name { get; private set; }

        internal static W3dEmitterHeader Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                return new W3dEmitterHeader
                {
                    Version = reader.ReadUInt32(),
                    Name = reader.ReadFixedLengthString(W3dConstants.NameLength)
                };
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
        }
    }
}
