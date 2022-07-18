using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dSoundRObjHeader : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_SOUNDROBJ_HEADER;

        public uint Version { get; private set; }

        public string Name { get; private set; }

        internal static W3dSoundRObjHeader Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dSoundRObjHeader
                {
                    Version = reader.ReadUInt32(),
                    Name = reader.ReadFixedLengthString((int) header.ChunkSize - 4)
                };

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.WriteFixedLengthString(Name, Name.Length + 1);
        }
    }
}
