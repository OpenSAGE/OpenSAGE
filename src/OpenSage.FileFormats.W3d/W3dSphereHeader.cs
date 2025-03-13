using System.IO;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSphereHeader(
    uint ChunkType,
    uint ChunkSize,
    uint Version,
    uint Flags,
    bool AlignedCamera,
    bool Looping,
    string Name,
    Vector3 Center,
    Vector3 Size,
    float Duration,
    ColorRgbF InitialColor,
    float InitialOpacity,
    Vector3 InitialScale,
    Vector3 InitialAlphaVector,
    Vector2 InitialAlphaVectorMagnitude,
    string TextureFileName)
{
    internal static W3dSphereHeader Parse(BinaryReader reader)
    {
        var chunkType = reader.ReadUInt32();
        var chunkSize = reader.ReadUInt32() & 0x7FFFFFFF;
        var version = reader.ReadUInt32();
        var flags = reader.ReadUInt32();

        var alignedCamera = ((flags & 1) == 1);
        var looping = ((flags & 2) == 2);
        var name = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);
        var center = reader.ReadVector3();
        var size = reader.ReadVector3();
        var duration = reader.ReadSingle();
        var initialColor = reader.ReadColorRgbF();
        var initialOpacity = reader.ReadSingle();
        var initialScale = reader.ReadVector3();
        var initialAlphaVector = reader.ReadVector3();
        var initialAlphaVectorMagnitude = reader.ReadVector2();
        var textureFileName = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);

        return new W3dSphereHeader(chunkType, chunkSize, version, flags, alignedCamera, looping, name, center, size,
            duration, initialColor, initialOpacity, initialScale, initialAlphaVector, initialAlphaVectorMagnitude,
            textureFileName);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.Write(Flags);
        writer.WriteFixedLengthString(Name, W3dConstants.NameLength * 2);
        writer.Write(Center);
        writer.Write(Size);
        writer.Write(Duration);
        writer.Write(InitialColor);
        writer.Write(InitialOpacity);
        writer.Write(InitialScale);
        writer.Write(InitialAlphaVector);
        writer.Write(InitialAlphaVectorMagnitude);
        writer.WriteFixedLengthString(TextureFileName, W3dConstants.NameLength * 2);
    }
}
