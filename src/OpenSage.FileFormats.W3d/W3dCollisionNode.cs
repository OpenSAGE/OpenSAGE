using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dCollisionNode(byte[] UnknownBytes) : W3dChunk(W3dChunkType.W3D_CHUNK_COLLISION_NODE)
{
    internal static W3dCollisionNode Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var unknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine W3dCollisionNode UnknownBytes

            return new W3dCollisionNode(unknownBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(UnknownBytes);
    }
}
