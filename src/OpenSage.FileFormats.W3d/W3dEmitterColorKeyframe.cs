using System.IO;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterColorKeyframe(float Time, ColorRgba Color)
{
    internal static W3dEmitterColorKeyframe Parse(BinaryReader reader)
    {
        var time = reader.ReadSingle();
        var color = reader.ReadColorRgba();

        return new W3dEmitterColorKeyframe(time, color);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(Color);
    }
}
