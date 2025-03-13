using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSphereOpacityInfo(
    uint ChunkType,
    uint ChunkSize,
    uint Version,
    List<W3dSphereOpacity> OpacityInfo)
{
    internal static W3dSphereOpacityInfo Parse(BinaryReader reader)
    {
        var chunkType = reader.ReadUInt32();
        var chunkSize = reader.ReadUInt32() & 0x7FFFFFFF;
        var version = reader.ReadUInt32();                      // ? Version or something else?
        var opacityInfo = new List<W3dSphereOpacity>();

        var arraySize = reader.ReadUInt32();

        var arrayCount = arraySize / 10; // 10 = Size of OpacityInfo Array Chunk + Header Info
        for (var i = 0; i < arrayCount; i++)
        {
            var opacity = W3dSphereOpacity.Parse(reader);
            opacityInfo.Add(opacity);
        }

        return new W3dSphereOpacityInfo(chunkType, chunkSize, version, opacityInfo);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ChunkType);
        writer.Write(ChunkSize);
        writer.Write(Version);

        foreach (var opacity in OpacityInfo)
        {
            opacity.Write(writer);
        }
    }
}
