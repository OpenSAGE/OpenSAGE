using System.IO;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dRingHeader(
    uint ChunkType,
    uint ChunkSize,
    uint Version,
    uint Flags,
    bool AlignedCamera,
    bool Looping,
    string Name,
    Vector3 Center,
    Vector3 Extent,
    float Duration,
    ColorRgbF InitialColor,
    float InitialOpacity,
    Vector2 InnerScale,
    Vector2 OuterScale,
    Vector2 InnerRadii,
    Vector2 OuterRadii,
    string TextureFileName)
{
    internal static W3dRingHeader Parse(BinaryReader reader)
    {
        var chunkType = reader.ReadUInt32();
        var chunkSize = reader.ReadUInt32() & 0x7FFFFFFF;
        var version = reader.ReadUInt32();
        var flags = reader.ReadUInt32();

        var alignedCamera = ((flags & 1) == 1);
        var looping = ((flags & 2) == 2);
        var name = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);
        var center = reader.ReadVector3();
        var extent = reader.ReadVector3();
        var duration = reader.ReadSingle();
        var initialColor = reader.ReadColorRgbF();
        var initialOpacity = reader.ReadSingle();
        var innerScale = reader.ReadVector2();
        var outerScale = reader.ReadVector2();
        var innerRadii = reader.ReadVector2();
        var outerRadii = reader.ReadVector2();
        var textureFileName = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);


        return new W3dRingHeader(chunkType, chunkSize, version, flags, alignedCamera, looping, name, center, extent,
            duration, initialColor, initialOpacity, innerScale, outerScale, innerRadii, outerRadii, textureFileName);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ChunkType);
        writer.Write(ChunkSize);
        writer.Write(Version);
        writer.Write(Flags);
        writer.WriteFixedLengthString(Name, W3dConstants.NameLength * 2);
        writer.Write(Center);
        writer.Write(Extent);
        writer.Write(Duration);
        writer.Write(InitialColor);
        writer.Write(InitialOpacity);
        writer.Write(InnerScale);
        writer.Write(OuterScale);
        writer.Write(InnerRadii);
        writer.Write(OuterRadii);
        writer.WriteFixedLengthString(TextureFileName, W3dConstants.NameLength * 2);
    }
}
