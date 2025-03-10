using System.IO;

namespace OpenSage.FileFormats.W3d;


public sealed record W3dTextureReplacerInfo(uint Version, byte[] UnknownBytes)
    : W3dChunk(W3dChunkType.W3D_CHUNK_TEXTURE_REPLACER_INFO)
{
    internal static W3dTextureReplacerInfo Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var version = reader.ReadUInt32();
            var unknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine W3dTextureReplacerInfo UnknownBytes.

            return new W3dTextureReplacerInfo(version, unknownBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.Write(UnknownBytes);
    }
}
