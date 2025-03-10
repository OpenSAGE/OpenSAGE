using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dMorphAnimHeader(byte[] UnknownBytes) : W3dChunk(W3dChunkType.W3D_CHUNK_MORPHANIM_HEADER)
{
    internal static W3dMorphAnimHeader Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var unknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine W3dMorphAnimHeader Chunk Structure (Currently Unknown)

            return new W3dMorphAnimHeader(unknownBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(UnknownBytes);
    }
}
