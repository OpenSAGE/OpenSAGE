using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSpherePlaceholder(uint ChunkType, uint ChunkSize)
{
    internal static W3dSpherePlaceholder Parse(BinaryReader reader)
    {
        var chunkType = reader.ReadUInt32();
        var chunkSize = reader.ReadUInt32() & 0x7FFFFFFF;

        return new W3dSpherePlaceholder(chunkType, chunkSize);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ChunkType);
        writer.Write(ChunkSize);
    }
}
