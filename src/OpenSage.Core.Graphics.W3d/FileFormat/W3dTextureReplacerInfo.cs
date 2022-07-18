using System.IO;

namespace OpenSage.FileFormats.W3d { 

    public sealed class W3dTextureReplacerInfo : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_TEXTURE_REPLACER_INFO;

        public uint Version { get; private set; }

        public byte[] UnknownBytes { get; private set; }

        internal static W3dTextureReplacerInfo Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dTextureReplacerInfo
                {
                    Version = reader.ReadUInt32(),
                    UnknownBytes = reader.ReadBytes((int) context.CurrentEndPosition - (int) reader.BaseStream.Position)
                };

                // TODO: Determine W3dTextureReplacerInfo UnknownBytes.

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(UnknownBytes);
        }
    }
}
