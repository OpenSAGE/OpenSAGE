using System;
using System.IO;
using Smx.SharpIO;

namespace OpenSage.FileFormats.Big
{
    public class BigArchiveEntryStream : Stream
    {
        private readonly BigArchiveEntry _entry;
        private readonly BigArchive _archive;
        private bool _write;
        private readonly Memory<byte> _mem;
        private readonly SpanStream _spanStream;

        public BigArchiveEntryStream(BigArchiveEntry entry, Memory<byte> mem)
        {
            _write = false;
            _entry = entry;
            _archive = entry.Archive;

            _mem = mem;
            _spanStream = new SpanStream(_mem);
        }

        public override void Flush()
        {
            if (_write)
            {
                _entry.Length = (uint) this.Length;
            }
            _archive.WriteToDisk();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _archive.AcquireLock();
            int result;
            if (_write == false)
            {
                if (count > (Length - Position))
                {
                    count = (int) (Length - Position);
                }

                result = _spanStream.Read(buffer, offset, count);
                Position += result;
            }
            else
            {
                result = _entry.OutstandingWriteStream.Read(buffer, offset, count);
                Position += result;
            }
            _archive.ReleaseLock();

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

        private void EnsureWriteMode()
        {
            if (_write == false)
            {
                _entry.OutstandingWriteStream = new MemoryStream();
                CopyTo(_entry.OutstandingWriteStream);
                _write = true;
            }
        }

        public override void SetLength(long value)
        {
            EnsureWriteMode();

            _entry.OnDisk = false;
            _entry.OutstandingWriteStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureWriteMode();

            _entry.OnDisk = false;
            _entry.OutstandingWriteStream.Position = Position;
            _entry.OutstandingWriteStream.Write(buffer, offset, count);
        }

        public override bool CanRead => _archive.Stream.CanRead;

        public override bool CanSeek => _archive.Stream.CanSeek;

        public override bool CanWrite => true;

        public override long Length => _write ? _entry.OutstandingWriteStream.Length : _entry.Length;

        private long _position = 0;

        public override long Position
        {
            get => _position;
            set
            {
                _position = value;
                _spanStream.Seek(Position, SeekOrigin.Begin);
            }
        }

        public override void Close()
        {
            Flush();
            base.Close();
        }
    }
}
