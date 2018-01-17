using System;
using System.IO;

namespace OpenSage.Data.RefPack
{
    // Clean-room implementation based on spec here:
    // http://wiki.niotso.org/RefPack#Bitstream_specification
    public class RefPackStream : Stream
    {
        private readonly Stream _stream;
        private readonly byte[] _output;
        private int _currentOutputPosition;
        private int _nextOutputPosition;
        private bool _eof;

        public override bool CanRead => _stream?.CanRead ?? false;

        public override bool CanWrite => false;

        /// <summary>
        /// Can only seek backwards from the current position.
        /// </summary>
        public override bool CanSeek => true;

        public override long Length => _output.Length;

        public override long Position
        {
            get => _currentOutputPosition;
            set => throw new NotImplementedException();
        }

        public static bool IsProbablyRefPackCompressed(Stream stream)
        {
            if (stream.Length < 2)
            {
                return false;
            }

            var position = stream.Position;
            try
            {
                var headerByte1 = stream.ReadByte();
                if ((headerByte1 & 0b00111110) != 0b00010000)
                {
                    return false;
                }

                var headerByte2 = stream.ReadByte();
                if (headerByte2 != 0b11111011)
                {
                    return false;
                }

                return true;
            }
            finally
            {
                stream.Position = position;
            }
        }

        public RefPackStream(Stream compressedStream)
        {
            _stream = compressedStream;

            // Read enough of stream to get uncompressed size.

            var headerByte1 = compressedStream.ReadByte();
            if ((headerByte1 & 0b00111110) != 0b00010000)
            {
                throw new InvalidDataException();
            }

            var largeFilesFlag = (headerByte1 & 0b10000000) != 0;
            var compressedSizePresent = (headerByte1 & 0b00000001) != 0;

            var headerByte2 = compressedStream.ReadByte();
            if (headerByte2 != 0b11111011)
            {
                throw new InvalidDataException();
            }

            int readBigEndianSize()
            {
                var count = largeFilesFlag ? 4 : 3;
                var size = 0;
                for (var i = 0; i < count; i++)
                {
                    size = (size << 8) | compressedStream.ReadByte();
                }
                return size;
            }

            if (compressedSizePresent)
            {
                var compressedSize = readBigEndianSize();
            }
            var decompressedSize = readBigEndianSize();

            _output = new byte[decompressedSize];
        }

        public override void Flush()
        {
            
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin != SeekOrigin.Current || offset > 0)
            {
                throw new NotSupportedException();
            }

            _currentOutputPosition += (int) offset;

            return _currentOutputPosition;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var actualCount = Math.Min(count, _output.Length - _currentOutputPosition);

            while (!_eof && _currentOutputPosition + actualCount > _nextOutputPosition)
            {
                ExecuteCommand();
            }

            Array.Copy(_output, _currentOutputPosition, buffer, offset, actualCount);
            _currentOutputPosition += actualCount;

            return actualCount;
        }

        private void ExecuteCommand()
        {
            var byte1 = _stream.ReadByte();
            if ((byte1 & 0x80) == 0) // 2-byte command
            {
                Execute2ByteCommand(byte1);
            }
            else if ((byte1 & 0x40) == 0) // 3-byte command
            {
                Execute3ByteCommand(byte1);
            }
            else if ((byte1 & 0x20) == 0) // 4-byte command
            {
                Execute4ByteCommand(byte1);
            }
            else // 1-byte command
            {
                if (byte1 < 0xFC) // Ordinary 1-byte command
                {
                    Execute1ByteCommand(byte1);
                }
                else // Stop command
                {
                    Execute1ByteCommandAndStop(byte1);
                }
            }
        }

        private void Execute2ByteCommand(int byte1)
        {
            var byte2 = _stream.ReadByte();

            var proceedingDataLength = byte1 & 0x03;
            CopyProceeding(proceedingDataLength);

            var referencedDataLength = ((byte1 & 0x1C) >> 2) + 3;
            var referencedDataDistance = ((byte1 & 0x60) << 3) + byte2 + 1;
            CopyReferencedData(referencedDataLength, referencedDataDistance);
        }

        private void Execute3ByteCommand(int byte1)
        {
            var byte2 = _stream.ReadByte();
            var byte3 = _stream.ReadByte();

            var proceedingDataLength = (byte2 & 0xC0) >> 6;
            CopyProceeding(proceedingDataLength);

            var referencedDataLength = (byte1 & 0x3F) + 4;
            var referencedDataDistance = ((byte2 & 0x3F) << 8) + byte3 + 1;
            CopyReferencedData(referencedDataLength, referencedDataDistance);
        }

        private void Execute4ByteCommand(int byte1)
        {
            var byte2 = _stream.ReadByte();
            var byte3 = _stream.ReadByte();
            var byte4 = _stream.ReadByte();

            var proceedingDataLength = byte1 & 0x03;
            CopyProceeding(proceedingDataLength);

            var referencedDataLength = ((byte1 & 0x0C) << 6) + byte4 + 5;
            var referencedDataDistance = ((byte1 & 0x10) << 12) + (byte2 << 8) + byte3 + 1;
            CopyReferencedData(referencedDataLength, referencedDataDistance);
        }

        private void Execute1ByteCommand(int byte1)
        {
            var proceedingDataLength = ((byte1 & 0x1F) + 1) << 2;
            CopyProceeding(proceedingDataLength);
        }

        private void Execute1ByteCommandAndStop(int byte1)
        {
            var proceedingDataLength = byte1 & 0x03;
            CopyProceeding(proceedingDataLength);

            _eof = true;
        }

        private void CopyProceeding(int proceedingDataLength)
        {
            var bytesRead = _stream.Read(_output, _nextOutputPosition, proceedingDataLength);
            if (bytesRead != proceedingDataLength)
            {
                throw new InvalidDataException();
            }
            _nextOutputPosition += proceedingDataLength;
        }

        private void CopyReferencedData(int referencedDataLength, int referencedDataDistance)
        {
            if (referencedDataDistance > _nextOutputPosition || referencedDataDistance <= 0)
            {
                throw new InvalidDataException();
            }

            // Max value for referencedDataDistance is 131072.
            // We use that fact to only keep that number of bytes around in the output buffer.
            // If this isn't the case, things will break.
            if (referencedDataDistance > 131072)
            {
                throw new InvalidDataException();
            }

            var referencedDataIndex = _nextOutputPosition - referencedDataDistance;

            // Copy bytes 1 at a time because it's valid for the referenced data pointer
            // to overrun into the initial value of the output data pointer.
            while (referencedDataLength-- > 0)
            {
                _output[_nextOutputPosition++] = _output[referencedDataIndex++];
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
