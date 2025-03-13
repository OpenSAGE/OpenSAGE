using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <summary>
/// Frames keyframes are for sub-texture indexing.
/// </summary>
public sealed record W3dEmitterFrameHeader(uint KeyframeCount, float Random)
{
    internal static W3dEmitterFrameHeader Parse(BinaryReader reader)
    {
        var keyframeCount = reader.ReadUInt32();
        var random = reader.ReadSingle();

        reader.ReadBytes(2 * sizeof(uint)); // Pad

        return new W3dEmitterFrameHeader(keyframeCount, random);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(KeyframeCount);
        writer.Write(Random);

        writer.Write((uint)0); // Pad
        writer.Write((uint)0);
    }
}
