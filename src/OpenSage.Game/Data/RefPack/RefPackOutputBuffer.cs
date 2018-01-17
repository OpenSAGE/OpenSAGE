using System;
using System.IO;

namespace OpenSage.Data.RefPack
{
    internal sealed class RefPackOutputBuffer
    {
        public const int MaxReferencedDataDistance = 131072;
        public const int MaxByteReadCount = MaxReferencedDataDistance * 300;
        public const int WindowSize = MaxReferencedDataDistance * 600;

        private readonly byte[] _buffer;

        public RefPackOutputBuffer(int maxLength)
        {
            _buffer = new byte[Math.Min(maxLength, WindowSize)];
        }

        public void CopyTo(int sourceOffset, byte[] destinationBuffer, int destinationOffset, int count)
        {
            if ((sourceOffset % WindowSize) + count > WindowSize)
            {
                // Need to read from end, then beginning, of buffer.
                var numBytesToReadAtEnd = WindowSize - (sourceOffset % WindowSize);
                Array.Copy(_buffer, sourceOffset % WindowSize, destinationBuffer, destinationOffset, numBytesToReadAtEnd);
                Array.Copy(_buffer, 0, destinationBuffer, destinationOffset + numBytesToReadAtEnd, count - numBytesToReadAtEnd);
            }
            else
            {
                Array.Copy(_buffer, sourceOffset % WindowSize, destinationBuffer, destinationOffset, count);
            }
        }

        public int ReadFrom(Stream stream, int offset, int count)
        {
            if ((offset % WindowSize) + count > WindowSize)
            {
                // Need to write to end, then beginning, of buffer.
                var numBytesToWriteAtEnd = WindowSize - (offset % WindowSize);
                var bytesRead = stream.Read(_buffer, offset % WindowSize, numBytesToWriteAtEnd);
                bytesRead += stream.Read(_buffer, 0, count - numBytesToWriteAtEnd);
                return bytesRead;
            }
            else
            {
                return stream.Read(_buffer, offset % WindowSize, count);
            }
        }

        public void CopyFromReferencedData(int referencedDataDistance, ref int destinationOffset, int count)
        {
            var referencedDataIndex = destinationOffset - referencedDataDistance;

            // As an optimisation, use a simple loop when referencedDataDistance is small
            // and the referenced data overlaps with the current position.
            if (referencedDataDistance < 3 && referencedDataDistance < count)
            {
                for (var i = 0; i < count; i++)
                {
                    _buffer[destinationOffset++ % WindowSize] = _buffer[referencedDataIndex++ % WindowSize];
                }
                return;
            }

            var totalBytesRemaining = count;
            while (totalBytesRemaining > 0)
            {
                var totalBytesToCopy = Math.Min(totalBytesRemaining, referencedDataDistance);

                var currentSourceOffset = referencedDataIndex % WindowSize;
                var currentDestinationOffset = destinationOffset % WindowSize;
                var numBytesRemaining = totalBytesToCopy;

                while (numBytesRemaining > 0)
                {
                    // Four possible states:
                    // 1. currentSourceOffset + numBytesRemaining is beyond the end of the window.
                    // 2. currentDestinationOffset + numBytesRemaining is beyond the end of the window.
                    // 3. Both 1 and 2.
                    // 4. currentSourceOffset + numBytesRemaining, and currentDestinationOffset + numBytesRemaining, are within the window.
                    // We can handle all these 4 cases with the following code.

                    var numBytesToCopy = Math.Min(
                        Math.Min(
                            WindowSize - currentSourceOffset,
                            WindowSize - currentDestinationOffset),
                        numBytesRemaining);

                    Array.Copy(_buffer, currentSourceOffset, _buffer, currentDestinationOffset, numBytesToCopy);

                    numBytesRemaining -= numBytesToCopy;

                    if (numBytesRemaining > 0)
                    {
                        currentSourceOffset = (currentSourceOffset + numBytesToCopy) % WindowSize;
                        currentDestinationOffset = (currentDestinationOffset + numBytesToCopy) % WindowSize;
                    }
                }

                destinationOffset += totalBytesToCopy;

                totalBytesRemaining -= totalBytesToCopy;
            }
        }
    }
}
