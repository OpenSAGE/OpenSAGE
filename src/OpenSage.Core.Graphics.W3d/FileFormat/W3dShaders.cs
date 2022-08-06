using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dShaders : W3dListChunk<W3dShaders, W3dShader>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_SHADERS;

        internal static W3dShaders Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseList(reader, context, W3dShader.Parse);
        }

        protected override void WriteItem(BinaryWriter writer, W3dShader item)
        {
            item.WriteTo(writer);
        }
    }
}
