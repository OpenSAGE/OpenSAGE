using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dMorphAnimKeyData : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_MORPHANIM_KEYDATA;

        public byte[] UnknownBytes { get; private set; }

        internal static W3dMorphAnimKeyData Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dMorphAnimKeyData
                {
                    UnknownBytes = reader.ReadBytes((int) context.CurrentEndPosition - (int) reader.BaseStream.Position)
                };

                // TODO: Determine W3dMorphAnimKeyData Chunk Structure (Currently Unknown)

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(UnknownBytes);
        }
    }
}
