using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterSizeKeyframe(float Time, float Size)
{
    internal static W3dEmitterSizeKeyframe Parse(BinaryReader reader)
    {
        var time = reader.ReadSingle();
        var size = reader.ReadSingle();

        return new W3dEmitterSizeKeyframe(time, size);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(Size);
    }
}
