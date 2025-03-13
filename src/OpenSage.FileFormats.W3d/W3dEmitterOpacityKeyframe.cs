using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterOpacityKeyframe(float Time, float Opacity)
{
    internal static W3dEmitterOpacityKeyframe Parse(BinaryReader reader)
    {
        var time = reader.ReadSingle();
        var opacity = reader.ReadSingle();

        return new W3dEmitterOpacityKeyframe(time, opacity);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(Opacity);
    }
}
