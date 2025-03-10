using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterBlurTimeHeader(uint KeyframeCount, float Random)
{
    internal static W3dEmitterBlurTimeHeader Parse(BinaryReader reader)
    {
        var keyframeCount = reader.ReadUInt32();
        var random = reader.ReadSingle();

        reader.ReadUInt32(); // Pad

        return new W3dEmitterBlurTimeHeader(keyframeCount, random);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(KeyframeCount);
        writer.Write(Random);
        writer.Write((uint)0); // Pad
    }
}
