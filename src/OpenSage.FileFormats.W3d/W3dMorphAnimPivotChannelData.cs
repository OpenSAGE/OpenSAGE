using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dMorphAnimPivotChannelData(byte[] UnknownBytes)
    : W3dChunk(W3dChunkType.W3D_CHUNK_MORPHANIM_PIVOTCHANNELDATA)
{
    internal static W3dMorphAnimPivotChannelData Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var unknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine W3dMorphAnimPivotChannelData Chunk Structure (Currently Unknown)

            return new W3dMorphAnimPivotChannelData(unknownBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(UnknownBytes);
    }
}
