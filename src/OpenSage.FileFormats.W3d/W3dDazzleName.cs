using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dDazzleName : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_DAZZLE_NAME;

        public string Name { get; private set; }

        internal static W3dDazzleName Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dDazzleName
                {
                    Name = reader.ReadFixedLengthString((int) context.CurrentEndPosition - (int) reader.BaseStream.Position)
                };

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.WriteFixedLengthString(Name, Name.Length + 1);
        }
    }
}
