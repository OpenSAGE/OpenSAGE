using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dShadowNode(byte[] UnknownBytes) : W3dChunk(W3dChunkType.OBSOLETE_W3D_CHUNK_SHADOW_NODE)
{
    internal static W3dShadowNode Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var unknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine W3dShadowNode UnknownBytes

            return new W3dShadowNode(unknownBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(UnknownBytes);
    }
}
