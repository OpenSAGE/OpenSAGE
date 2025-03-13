using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dMorphAnimKeyData(byte[] UnknownBytes) : W3dChunk(W3dChunkType.W3D_CHUNK_MORPHANIM_KEYDATA)
{
    internal static W3dMorphAnimKeyData Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var unknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine W3dMorphAnimKeyData Chunk Structure (Currently Unknown)

            return new W3dMorphAnimKeyData(unknownBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(UnknownBytes);
    }
}
