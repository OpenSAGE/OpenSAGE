using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dAnimationHeader(uint Version, string Name, string HierarchyName, uint NumFrames, uint FrameRate)
    : W3dChunk(W3dChunkType.W3D_CHUNK_ANIMATION_HEADER)
{
    internal static W3dAnimationHeader Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var version = reader.ReadUInt32();
            var name = reader.ReadFixedLengthString(W3dConstants.NameLength);
            var hierarchyName = reader.ReadFixedLengthString(W3dConstants.NameLength);
            var numFrames = reader.ReadUInt32();
            var frameRate = reader.ReadUInt32();

            return new W3dAnimationHeader(version, name, hierarchyName, numFrames, frameRate);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
        writer.WriteFixedLengthString(HierarchyName, W3dConstants.NameLength);
        writer.Write(NumFrames);
        writer.Write(FrameRate);
    }
}
