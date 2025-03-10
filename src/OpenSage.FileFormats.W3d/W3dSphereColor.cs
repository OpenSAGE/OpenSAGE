using System.IO;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSphereColor(
    uint ChunkType,
    uint ChunkSize,
    ColorRgbF Color,
    float Position)
{
    internal static W3dSphereColor Parse(BinaryReader reader)
    {
        var chunkType = reader.ReadByte();
        var chunkSize = reader.ReadByte();
        var color = reader.ReadColorRgbF();
        var position = reader.ReadSingle();

        return new W3dSphereColor(chunkType, chunkSize, color, position);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ChunkType);
        writer.Write(ChunkSize);
        writer.Write(Color);
        writer.Write(Position);
    }
}
