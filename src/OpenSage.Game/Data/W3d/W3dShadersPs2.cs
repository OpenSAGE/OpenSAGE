using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dShadersPs2 : W3dListChunk<W3dShadersPs2, W3dShaderPs2>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_PS2_SHADERS;

        internal static W3dShadersPs2 Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseList(reader, context, W3dShaderPs2.Parse);
        }

        protected override void WriteItem(BinaryWriter writer, W3dShaderPs2 item)
        {
            item.WriteTo(writer);
        }
    }
}
