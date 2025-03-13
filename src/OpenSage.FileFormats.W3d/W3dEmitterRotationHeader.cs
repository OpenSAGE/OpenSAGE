using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="KeyframeCount"></param>
/// <param name="Random">Random initial rotational velocity (rotations/sec)</param>
/// <param name="OrientationRandom">Random initial orientation (rotations 1.0=360deg)</param>
public sealed record W3dEmitterRotationHeader(uint KeyframeCount, float Random, float OrientationRandom)
{
    internal static W3dEmitterRotationHeader Parse(BinaryReader reader)
    {
        var keyframeCount = reader.ReadUInt32();
        var random = reader.ReadSingle();
        var orientationRandom = reader.ReadSingle();

        reader.ReadBytes(sizeof(uint)); // Pad

        return new W3dEmitterRotationHeader(keyframeCount, random, orientationRandom);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(KeyframeCount);
        writer.Write(Random);
        writer.Write(OrientationRandom);

        writer.Write((uint)0); // Pad
    }
}
