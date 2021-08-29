using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt
{
    public sealed class MemoryPool: IDisposable
    {
        private Dictionary<uint, (MemoryPool, uint)> _preMapping;
        private Dictionary<uint, uint> _postMapping;
        private MemoryStream _stream;
        private BinaryWriter _writer;

        public BinaryWriter Writer => _writer;

        private MemoryPool _post = null;
        private MemoryPool _pre = null;

        private MemoryPool _postAutoCreate
        {
            get
            {
                if (_post == null)
                    _post = new(this);
                return _post;
            }
        }

        public MemoryPool Pre => _pre;
        public MemoryPool Post => _postAutoCreate;

        public MemoryPool() : this(null) { }

        public MemoryPool(MemoryPool pre) 
        {
            _pre = pre;
            _preMapping = new();
            _postMapping = new();
            _stream = new();
            _writer = new(_stream);
        }

        public void Dispose()
        {
            if (_post != null)
                _post.Dispose();
            _writer.Dispose();
            _stream.Dispose();
        }

        /// <summary>
        /// (this, cur_offset) -> (target, offset)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public uint RegisterPreOffset(MemoryPool target, uint offset)
        {
            var ret = (uint) _stream.Position;
            target._preMapping[offset] = (this, ret);
            return ret;
        }

        public (MemoryPool, uint) RemovePreOffset(MemoryPool target, uint offset)
        {
            var ret = target._preMapping[offset];
            target._preMapping.Remove(offset);
            return ret;
        }

        /// <summary>
        /// (pre, offset) -> (this, reg_offset)
        /// reg_offset is assigned to _stream.Position if negative
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="reg_offset"></param>
        /// <returns></returns>
        public uint RegisterPostOffset(uint offset, long reg_offset = -1)
        {
            if (reg_offset < 0)
                reg_offset = _stream.Position;
            _postMapping[offset] = (uint) reg_offset;
            return _postMapping[offset];
        }

        public uint RemovePostOffset(uint offset)
        {
            var ret = _postMapping[offset];
            _postMapping.Remove(offset);
            return ret;
        }

        public uint RemovePostOffsetAndSeek(uint offset)
        {
            var ret = RemovePostOffset(offset);
            _stream.Seek(ret, SeekOrigin.Begin);
            return ret;
        }

        public void WriteStringAtOffset(uint offset, string val)
        {
            RegisterPostOffset(offset);
            _writer.WriteNullTerminatedString(val);
        }

        public void WriteArrayAtOffset(uint offset, int length, Func<int, BinaryWriter, MemoryPool, bool> action)
        {
            RegisterPostOffset(offset);
            for (int i = 0; i < length; ++i)
            {
                var flag = action(i, _writer, _postAutoCreate);
                // TODO what if flag == false?
                // The design is that action handles it
            }
        }

        public void WritePointerArrayAtOffset(uint offset, int length, Func<int, BinaryWriter, MemoryPool, bool> action)
        {
            RegisterPostOffset(offset);
            for (int i = 0; i < length; ++i)
            {
                var cur_offset = (uint) _stream.Position;
                _writer.Write((UInt32) 0);
                var post = _postAutoCreate;
                post.RegisterPostOffset(cur_offset);
                var flag = action(i, post._writer, post._postAutoCreate);
                if (!flag)
                    post.RemovePostOffsetAndSeek(cur_offset);
            }
        }

        /// <summary>
        /// STRONGLY NOT RECOMMENDED TO CALL although it is public. 
        /// Use BinaryIOExtensions.DumpMemoryPool Instead. 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="offset"></param>
        /// <param name="offsets"></param>
        public void SerializeToFile(BinaryWriter writer, uint offset = 0, Dictionary<MemoryPool, uint> offsets = null)
        {
            var fix_block_length = (uint) _stream.Position;
            if (fix_block_length == 0) return;

            if (offsets == null)
                offsets = new() { [null] = offset };
            offsets[this] = offset;

            // the last pool muse be serialized first
            if (_post != null)
                _post.SerializeToFile(writer, offset + fix_block_length, offsets);

            // copy anything from the data chunk to the file
            _stream.Seek(0, SeekOrigin.Begin);
            writer.BaseStream.Seek(offset, SeekOrigin.Begin);
            _stream.CopyToAsync(writer.BaseStream);
            _stream.Seek(fix_block_length, SeekOrigin.Begin);

            // set all post pointers to correct positions
            if (_post != null)
                foreach (var kvp in _post._postMapping)
                {
                    writer.BaseStream.Seek(offset + kvp.Key, SeekOrigin.Begin);
                    writer.Write((UInt32) (offset + fix_block_length + kvp.Value));
                }

            // set all pre pointers
            foreach (var kvp in _preMapping)
            {
                writer.BaseStream.Seek(offset + kvp.Key, SeekOrigin.Begin);
                writer.Write((UInt32) (offsets[kvp.Value.Item1] + kvp.Value.Item2));
            }
        }

    };
}
