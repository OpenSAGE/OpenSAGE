using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt
{
    public sealed class MemoryPool
    {
        private Dictionary<uint, uint> _fixMapping;
        private MemoryStream _fixStream;
        private BinaryWriter _fixWriter;

        public BinaryWriter Writer => _fixWriter;

        private MemoryPool _smallerPool = null;

        private MemoryPool _smallerPoolAutoCreate
        {
            get
            {
                if (_smallerPool == null)
                    _smallerPool = new();
                return _smallerPool;
            }
        }

        public MemoryPool SmallerPool => _smallerPoolAutoCreate;

        public MemoryPool() 
        {
            _fixMapping = new();
            _fixStream = new();
            _fixWriter = new(_fixStream);
        }

        public uint RegisterFixedOffset(uint offset)
        {
            _fixMapping[offset] = (uint) _fixStream.Position;
            return _fixMapping[offset];
        }

        public uint RemoveFixedOffset(uint offset)
        {
            var ret = _fixMapping[offset];
            _fixMapping.Remove(offset);
            return ret;
        }


        public void WriteStringAtOffset(uint offset, string val)
        {
            RegisterFixedOffset(offset);
            _fixWriter.WriteNullTerminatedString(val);
        }

        public void WriteArrayAtOffset(uint offset, int length, Func<int, BinaryWriter, MemoryPool, bool> action)
        {
            RegisterFixedOffset(offset);
            for (int i = 0; i < length; ++i)
                action(i, _fixWriter, _smallerPoolAutoCreate);
        }

        public void WritePointerArrayAtOffset(uint offset, int length, Func<int, BinaryWriter, MemoryPool, bool> action)
        {
            RegisterFixedOffset(offset);
            for (int i = 0; i < length; ++i)
            {
                var cur_offset = (uint) _fixStream.Position;
                _fixWriter.Write((UInt32) 0);
                var sp = _smallerPoolAutoCreate;
                sp.RegisterFixedOffset(cur_offset);
                var flag = action(i, sp._fixWriter, sp._smallerPoolAutoCreate);
                if (!flag)
                    sp._fixStream.Seek(sp.RemoveFixedOffset(cur_offset), SeekOrigin.Begin);
            }
        }

        public void SerializeToFile(BinaryWriter writer, uint offsetFromFileStart)
        {
            var fix_block_length = (uint) _fixStream.Position;
            if (fix_block_length == 0) return;

            // the last pool muse be serialized first
            if (_smallerPool != null)
                _smallerPool.SerializeToFile(writer, offsetFromFileStart + fix_block_length);

            // copy anything from the data chunk to the file
            _fixStream.Seek(0, SeekOrigin.Begin);
            writer.BaseStream.Seek(offsetFromFileStart, SeekOrigin.Begin);
            _fixStream.CopyToAsync(writer.BaseStream);
            _fixStream.Seek(fix_block_length, SeekOrigin.Begin);

            // set all pointers to correct positions
            if (_smallerPool != null)
                foreach (var kvp in _smallerPool._fixMapping)
                {
                    writer.BaseStream.Seek(offsetFromFileStart + kvp.Key, SeekOrigin.Begin);
                    writer.Write((UInt32) (offsetFromFileStart + fix_block_length + kvp.Value));
                }
        }

    };
}
