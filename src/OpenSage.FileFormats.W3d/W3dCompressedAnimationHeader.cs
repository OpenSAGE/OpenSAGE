using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dCompressedAnimationHeader(
    uint Version,
    string Name,
    string HierarchyName,
    uint NumFrames,
    ushort FrameRate,
    W3dCompressedAnimationFlavor Flavor) : W3dChunk(W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_HEADER)
{
    internal static W3dCompressedAnimationHeader Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var version = reader.ReadUInt32();
            var name = reader.ReadFixedLengthString(W3dConstants.NameLength);
            var hierarchyName = reader.ReadFixedLengthString(W3dConstants.NameLength);
            var numFrames = reader.ReadUInt32();
            var frameRate = reader.ReadUInt16();
            var flavor = reader.ReadUInt16AsEnum<W3dCompressedAnimationFlavor>();

            return new W3dCompressedAnimationHeader(version, name, hierarchyName, numFrames, frameRate, flavor);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
        writer.WriteFixedLengthString(HierarchyName, W3dConstants.NameLength);
        writer.Write(NumFrames);
        writer.Write(FrameRate);
        writer.Write((ushort)Flavor);
    }
}
