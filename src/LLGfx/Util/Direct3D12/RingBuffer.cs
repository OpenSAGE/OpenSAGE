using System.Collections.Generic;

namespace LLGfx.Util
{
    internal class RingBuffer
    {
        private struct FrameTailAttributes
        {
            public long FrameNumber;
            public uint Offset;
            public uint Size;
        }
        private readonly Queue<FrameTailAttributes> _completedFrameTails;

        private uint _head;
        private uint _tail;
        private uint _usedSize;
        private uint _currentFrameSize;

        public uint MaxSize { get; }

        public bool IsFull => _usedSize == MaxSize;
        public bool IsEmpty => _usedSize == 0;
        public uint UsedSize => _usedSize;

        public RingBuffer(uint maxSize)
        {
            MaxSize = maxSize;
            _completedFrameTails = new Queue<FrameTailAttributes>();
        }

        protected uint? Allocate(uint size)
        {
            if (IsFull)
            {
                return null;
            }

            if (_tail >= _head)
            {
                if (_tail + size <= MaxSize)
                {
                    // Allocate from end of buffer.
                    var offset = _tail;
                    _tail += size;
                    _usedSize += size;
                    _currentFrameSize += size;
                    return offset;
                }
                else if (size <= _head)
                {
                    // Allocate from beginning of buffer.
                    var totalSize = (MaxSize - _tail) + size;
                    _usedSize += totalSize;
                    _currentFrameSize += totalSize;
                    _tail = size;
                    return 0;
                }
            }
            else if (_tail + size <= _head)
            {
                // Allocate from middle of buffer.
                var offset = _tail;
                _tail += size;
                _usedSize += size;
                _currentFrameSize += size;
                return offset;
            }

            return null;
        }

        public void FinishCurrentFrame(long frameNumber)
        {
            _completedFrameTails.Enqueue(new FrameTailAttributes
            {
                FrameNumber = frameNumber,
                Offset = _tail,
                Size = _currentFrameSize
            });
            _currentFrameSize = 0;
        }

        public void ReleaseCompletedFrames(long numCompletedFrames)
        {
            while (_completedFrameTails.Count > 0 && _completedFrameTails.Peek().FrameNumber < numCompletedFrames)
            {
                var oldestFrameTail = _completedFrameTails.Dequeue();
                _usedSize -= oldestFrameTail.Size;
                _head = oldestFrameTail.Offset;
            }
        }
    }
}
