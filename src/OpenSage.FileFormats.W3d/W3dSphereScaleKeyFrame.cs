using System.IO;
using System.Numerics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSphereScaleKeyFrame(
    uint ChunkType,
    uint ChunkSize,
    Vector3 ScaleKeyFrame,
    float Position)
{
    internal static W3dSphereScaleKeyFrame Parse(BinaryReader reader)
    {
        var chunkType = reader.ReadByte();
        var chunkSize = reader.ReadByte();
        var scaleKeyFrame = reader.ReadVector3();
        var position = reader.ReadSingle();

        return new W3dSphereScaleKeyFrame(chunkType, chunkSize, scaleKeyFrame, position);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ChunkType);
        writer.Write(ChunkSize);
        writer.Write(ScaleKeyFrame);
        writer.Write(Position);
    }
}
