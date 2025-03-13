using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="ChunkSize">Size of the chunk, (not including the chunk header)</param>
/// <param name="HasSubChunks"></param>
public sealed record W3dChunkHeader(uint ChunkSize, bool HasSubChunks)
{
    public const int SizeInBytes = sizeof(uint);

    internal static W3dChunkHeader Parse(BinaryReader reader)
    {
        var chunkSize = reader.ReadUInt32();
        chunkSize &= 0x7FFFFFFF;
        var hasSubChunks = (chunkSize >> 31) == 1;

        return new W3dChunkHeader(chunkSize, hasSubChunks);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        var chunkSize = ChunkSize;

        if (HasSubChunks)
        {
            chunkSize |= (1u << 31);
        }

        writer.Write(chunkSize);
    }
}
