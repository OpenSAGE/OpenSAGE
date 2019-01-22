using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dTextureName : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_TEXTURE_NAME;

        public string Value { get; private set; }

        internal static W3dTextureName Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                return new W3dTextureName
                {
                    Value = reader.ReadFixedLengthString((int) header.ChunkSize)
                };
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.WriteFixedLengthString(Value, Value.Length + 1);
        }
    }
}
