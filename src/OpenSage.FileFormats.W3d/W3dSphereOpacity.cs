using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSphereOpacity(
    uint ChunkType,
    uint ChunkSize,
    float Opacity,
    float Position)
{
    internal static W3dSphereOpacity Parse(BinaryReader reader)
    {
        var chunkType = reader.ReadByte();
        var chunkSize = reader.ReadByte();
        var opacity = reader.ReadSingle();
        var position = reader.ReadSingle();

        return new W3dSphereOpacity(chunkType, chunkSize, opacity, position);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ChunkType);
        writer.Write(ChunkSize);
        writer.Write(Opacity);
        writer.Write(Position);
    }
}
