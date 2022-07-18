using System.Collections.Generic;

namespace OpenSage.FileFormats.W3d
{
    internal sealed class W3dParseContext
    {
        private readonly Stack<ChunkStackEntry> _chunkParsingStack;

        public long CurrentEndPosition => _chunkParsingStack.Peek().EndPosition;

        public W3dParseContext()
        {
            _chunkParsingStack = new Stack<ChunkStackEntry>();
        }

        public void PushChunk(string chunkType, long endPosition)
        {
            _chunkParsingStack.Push(new ChunkStackEntry
            {
                ChunkType = chunkType,
                EndPosition = endPosition
            });
        }

        public void PopAsset()
        {
            _chunkParsingStack.Pop();
        }

        private struct ChunkStackEntry
        {
            public string ChunkType;
            public long EndPosition;
        }
    }
}
