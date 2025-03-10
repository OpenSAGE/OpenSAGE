using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="Time"></param>
/// <param name="Rotation">Rotational velocity in rotations/sec</param>
public sealed record W3dEmitterRotationKeyframe(float Time, float Rotation)
{
    internal static W3dEmitterRotationKeyframe Parse(BinaryReader reader)
    {
        var time = reader.ReadSingle();
        var rotation = reader.ReadSingle();

        return new W3dEmitterRotationKeyframe(time, rotation);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(Rotation);
    }
}
