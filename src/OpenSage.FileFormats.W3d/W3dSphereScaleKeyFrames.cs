using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSphereScaleKeyFrames(
    uint ChunkType,
    uint ChunkSize,
    uint Version,
    List<W3dSphereScaleKeyFrame> ScaleKeyFrames)
{
    internal static W3dSphereScaleKeyFrames Parse(BinaryReader reader)
    {
        var chunkType = reader.ReadUInt32();
        var chunkSize = reader.ReadUInt32() & 0x7FFFFFFF;
        var version = reader.ReadUInt32();
        var scaleKeyFrames = new List<W3dSphereScaleKeyFrame>();

        var arraySize = reader.ReadUInt32();

        var arrayCount = arraySize / 18; // 18 = Size of OpacityInfo Array Chunk + Header Info
        for (var i = 0; i < arrayCount; i++)
        {
            var keyFrame = W3dSphereScaleKeyFrame.Parse(reader);
            scaleKeyFrames.Add(keyFrame);
        }

        return new W3dSphereScaleKeyFrames(chunkType, chunkSize, version, scaleKeyFrames);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ChunkType);
        writer.Write(ChunkSize);
        writer.Write(Version);

        foreach (var keyFrame in ScaleKeyFrames)
        {
            keyFrame.Write(writer);
        }
    }
}
