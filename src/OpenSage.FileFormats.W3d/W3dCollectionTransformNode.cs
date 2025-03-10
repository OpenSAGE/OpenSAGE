using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dCollectionTransformNode(string Name) : W3dChunk(W3dChunkType.W3D_CHUNK_TRANSFORM_NODE)
{
    internal static W3dCollectionTransformNode Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var name = reader.ReadFixedLengthString((int)header.ChunkSize);

            return new W3dCollectionTransformNode(name);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.WriteFixedLengthString(Name, Name.Length + 1);
    }
}
