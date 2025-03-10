using System.Collections.Generic;

namespace OpenSage.FileFormats.W3d;

internal sealed class W3dParseContext
{
    private readonly Stack<ChunkStackEntry> _chunkParsingStack = new();

    public long CurrentEndPosition => _chunkParsingStack.Peek().EndPosition;

    public void PushChunk(string chunkType, long endPosition)
    {
        _chunkParsingStack.Push(new ChunkStackEntry(chunkType, endPosition));
    }

    public void PopAsset()
    {
        _chunkParsingStack.Pop();
    }

    private readonly record struct ChunkStackEntry(string ChunkType, long EndPosition);
}
