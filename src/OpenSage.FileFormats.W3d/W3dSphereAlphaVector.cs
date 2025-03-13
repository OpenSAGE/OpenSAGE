using System.IO;
using System.Numerics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSphereAlphaVector(
    uint ChunkType,
    uint ChunkSize,
    Vector3 Vector,
    Vector2 Magnitude,
    float Position)
{
    internal static W3dSphereAlphaVector Parse(BinaryReader reader)
    {
        var chunkType = reader.ReadByte();
        var chunkSize = reader.ReadByte();
        var vector = reader.ReadVector3();
        var magnitude = reader.ReadVector2();
        var position = reader.ReadSingle();

        return new W3dSphereAlphaVector(chunkType, chunkSize, vector, magnitude, position);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ChunkType);
        writer.Write(ChunkSize);
        writer.Write(Vector);
        writer.Write(Magnitude);
        writer.Write(Position);
    }
}
