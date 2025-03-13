using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterFrameKeyframe(float Time, float Frame)
{
    internal static W3dEmitterFrameKeyframe Parse(BinaryReader reader)
    {
        var time = reader.ReadSingle();
        var frame = reader.ReadSingle();

        return new W3dEmitterFrameKeyframe(time, frame);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(Frame);
    }
}
