using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSphereColors(
    uint ChunkType,
    uint ChunkSize,
    uint Version,
    List<W3dSphereColor> Colors)
{
    internal static W3dSphereColors Parse(BinaryReader reader)
    {
        var chunkType = reader.ReadUInt32();
        var chunkSize = reader.ReadUInt32() & 0x7FFFFFFF;
        var version = reader.ReadUInt32();                      // ? Version or something else?
        var colors = new List<W3dSphereColor>();

        var arraySize = reader.ReadUInt32();

        var arrayCount = arraySize / 18; // 18 = Size of Color Array Chunk + Header Info
        for (var i = 0; i < arrayCount; i++)
        {
            var color = W3dSphereColor.Parse(reader);
            colors.Add(color);
        }

        return new W3dSphereColors(chunkType, chunkSize, version, colors);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ChunkType);
        writer.Write(ChunkSize);
        writer.Write(Version);

        foreach (var color in Colors)
        {
            color.Write(writer);
        }
    }
}
