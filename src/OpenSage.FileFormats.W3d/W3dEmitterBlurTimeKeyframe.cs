using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterBlurTimeKeyframe(float Time, float BlurTime)
{
    internal static W3dEmitterBlurTimeKeyframe Parse(BinaryReader reader)
    {
        var time = reader.ReadSingle();
        var blurTime = reader.ReadSingle();

        return new W3dEmitterBlurTimeKeyframe(time, blurTime);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(BlurTime);
    }
}
