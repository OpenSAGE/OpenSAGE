using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace OpenSage.Tools.AptEditor.Util
{
    public sealed class MemoryChunk
    {
        public readonly uint StartAddress;
        public uint EndAddress { get { return StartAddress + (uint)Memory.Length; } }
        public byte[] Memory { get; private set; }
        public MemoryChunk(uint startAddress, uint bytes)
        {
            StartAddress = startAddress;
            Memory = new byte[bytes];
        }
    }

    public sealed class MemoryPool
    {
        // holds: (start address, memory chunk)
        private List<MemoryChunk> _memoryChunks;

        public MemoryPool()
        {
            _memoryChunks = new List<MemoryChunk>();
        }

        public uint LastEndAddress()
        {
            if (_memoryChunks.Count == 0)
            {
                return 0;
            }
            return _memoryChunks.Last().EndAddress;
        }

        public MemoryChunk Allocate(uint bytesCount)
        {
            _memoryChunks.Add(new MemoryChunk(LastEndAddress(), bytesCount));
            return _memoryChunks.Last();
        }

        public MemoryChunk AllocateBytesForPadding(uint nextChunkNeedsToAlignAs)
        {
            var lastEndAddress = LastEndAddress();
            if(lastEndAddress % nextChunkNeedsToAlignAs == 0)
            {
                return Allocate(0);
            }

            return Allocate(nextChunkNeedsToAlignAs - (lastEndAddress % nextChunkNeedsToAlignAs));
        }

        public MemoryChunk AllocatePadded(uint bytesCount, uint alignAs)
        {
            AllocateBytesForPadding(alignAs);
            return Allocate(bytesCount);
        }

        public MemoryChunk WriteDataToNewChunk(byte[] data)
        {
            var chunk = Allocate((uint)data.Length);
            data.CopyTo(chunk.Memory, 0);
            return chunk;
        }

        public MemoryStream GetMemoryStream()
        {
            if (_memoryChunks.Count == 0)
            {
                return new MemoryStream(new byte[0]);
            }

            var stream = new MemoryStream();
            foreach (var chunk in _memoryChunks)
            {
                stream.Write(chunk.Memory);
            }
            return stream;
        }
    };
}