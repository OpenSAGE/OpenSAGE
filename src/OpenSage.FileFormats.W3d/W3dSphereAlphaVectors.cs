using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSphereAlphaVectors(
    uint ChunkType,
    uint ChunkSize,
    uint Version,
    List<W3dSphereAlphaVector> AlphaVectors)
{
    internal static W3dSphereAlphaVectors Parse(BinaryReader reader)
    {
        var chunkType = reader.ReadUInt32();
        var chunkSize = reader.ReadUInt32() & 0x7FFFFFFF;
        var version = reader.ReadUInt32();
        var alphaVectors = new List<W3dSphereAlphaVector>();

        var arraySize = reader.ReadUInt32();

        var arrayCount = arraySize / 26; // 26 = Size of OpacityInfo Array Chunk + Header Info
        for (var i = 0; i < arrayCount; i++)
        {
            var alphaVector = W3dSphereAlphaVector.Parse(reader);
            alphaVectors.Add(alphaVector);
        }

        return new W3dSphereAlphaVectors(chunkType, chunkSize, version, alphaVectors);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ChunkType);
        writer.Write(ChunkSize);
        writer.Write(Version);

        foreach (var alphaVector in AlphaVectors)
        {
            alphaVector.Write(writer);
        }
    }
}
