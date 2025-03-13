using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dCollectionObjName(string[] Names) : W3dChunk(W3dChunkType.W3D_CHUNK_COLLECTION_OBJ_NAME)
{
    internal static W3dCollectionObjName Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var nameCount = header.ChunkSize / W3dConstants.NameLength;

            var names = new string[nameCount];
            for (var i = 0; i < nameCount; i++)
            {
                names[i] = reader.ReadFixedLengthString(W3dConstants.NameLength);
            }

            return new W3dCollectionObjName(names);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        for (var i = 0; i < Names.Length; i++)
        {
            writer.WriteFixedLengthString(Names[i], W3dConstants.NameLength);
        }
    }
}
