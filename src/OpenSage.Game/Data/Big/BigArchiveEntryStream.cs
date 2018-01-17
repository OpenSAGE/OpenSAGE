using System;
using System.IO;

namespace OpenSage.Data.Big
{
    public class BigArchiveEntryStream : Stream
    {
        private readonly BigArchiveEntry _entry;
        private readonly BigArchive _archive;
        private readonly uint _offset;
        private bool _locked;

        public BigArchiveEntryStream(BigArchiveEntry entry, uint offset)
        {
            _entry = entry;
            _archive = entry.Archive;
            _offset = offset;

            _archive.AcquireLock();
            _locked = true;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _archive.Stream.Seek(_offset + Position, SeekOrigin.Begin);
            if (count > (Length - Position))
            {
                count = (int) (Length - Position);
            }

            var result = _archive.Stream.Read(buffer, offset, count);
            Position += result;

            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    Position = Length + offset;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => _archive.Stream.CanRead;

        public override bool CanSeek => _archive.Stream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => _entry.Length;

        public override long Position { get; set; }

        public override void Close()
        {
            if (_locked)
            {
                _archive.ReleaseLock();
                _locked = false;
            }

            base.Close();
        }
    }
}
