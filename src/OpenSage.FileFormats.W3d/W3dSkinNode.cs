using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSkinNode(byte[] UnknownBytes) : W3dChunk(W3dChunkType.W3D_CHUNK_SKIN_NODE)
{
    internal static W3dSkinNode Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var unknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine W3dSkinNode UnknownBytes

            return new W3dSkinNode(unknownBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(UnknownBytes);
    }
}
