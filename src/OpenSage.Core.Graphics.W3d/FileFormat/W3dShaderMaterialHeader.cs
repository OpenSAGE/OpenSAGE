using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dShaderMaterialHeader : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_HEADER;

        public byte Number { get; private set; }
        public string TypeName { get; private set; }

        internal static W3dShaderMaterialHeader Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dShaderMaterialHeader
                {
                    Number = reader.ReadByte(),
                    TypeName = reader.ReadFixedLengthString(W3dConstants.NameLength * 2)
                };

                reader.ReadUInt32(); // Reserved

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(Number);
            writer.WriteFixedLengthString(TypeName, W3dConstants.NameLength * 2);

            writer.Write(0u); // Reserved
        }
    }
}
