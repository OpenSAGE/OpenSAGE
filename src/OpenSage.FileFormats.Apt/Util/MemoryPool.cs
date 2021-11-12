using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt
{
    public sealed class MemoryPool: IDisposable
    {
        public delegate (uint, uint) AlignedObjectWriter(BinaryWriter writer, MemoryPool pool);

        private readonly Dictionary<uint, (MemoryPool, uint)> _preMapping;
        private readonly Dictionary<uint, (MemoryPool, uint)> _preMapping2;
        private readonly Dictionary<uint, AlignedObjectWriter> _globalAlign;
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
            _preMapping2 = _pre == null ? new() : _pre._preMapping;
            _globalAlign = new();
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
            var pm = target == null ? _preMapping2 : target._preMapping;
            pm[offset] = (this, ret);
            return ret;
        }

        /*
        public (MemoryPool, uint) RemovePreOffset(MemoryPool target, uint offset)
        {
            var ret = target._preMapping[offset];
            var pm = target == null ? _preMapping2 : target._preMapping;
            pm.Remove(offset);
            return ret;
        }
        */

        /// <summary>
        /// (pre, offset) -> (global, ???) : objWrite
        /// </summary>
        /// <param name="objWrite"></param>
        /// <returns></returns>
        public uint RegisterGlobalAlignObject(uint offset, AlignedObjectWriter objWrite)
        {
            var ret = (uint) _stream.Position;
            _globalAlign[offset] = objWrite;
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
                var post = _postAutoCreate;
                post.RegisterPostOffset(cur_offset);
                var flag = action(i, post._writer, post._postAutoCreate);
                if (!flag)
                    post.RemovePostOffsetAndSeek(cur_offset);
                _writer.Write((UInt32) 0);
            }
        }

        /// <summary>
        /// STRONGLY NOT RECOMMENDED TO CALL although it is public. 
        /// Use BinaryIOExtensions.DumpMemoryPool Instead. 
        /// </summary>
        /// <param name="writer">the writer of the most outer layer (usually a file writer).</param>
        /// <param name="offset">the offset from file start to pool start.</param>
        /// <param name="offsets"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        public Dictionary<uint, AlignedObjectWriter> SerializeAndGatherAlignment(
            BinaryWriter writer,
            uint offset = 0,
            Dictionary<MemoryPool, uint> offsets = null,
            Dictionary<uint, AlignedObjectWriter> align = null
            )
        {
            // init align
            if (align == null)
                align = new();

            var fix_block_length = (uint) _stream.Position;
            if (fix_block_length == 0) return align;

            // init offset
            if (offsets == null)
                offsets = new();
            offsets[this] = offset;

            // the last pool muse be serialized first
            if (_post != null)
                _post.SerializeAndGatherAlignment(writer, offset + fix_block_length, offsets, align);

            // copy anything from the data chunk to the file
            _stream.Seek(0, SeekOrigin.Begin);
            writer.BaseStream.Seek(offset, SeekOrigin.Begin);
            _stream.CopyTo(writer.BaseStream);
            _stream.Seek(fix_block_length, SeekOrigin.Begin);

            // set all post pointers to correct positions
            if (_post != null)
                foreach (var kvp in _post._postMapping) // this[key] -> _post[val]
                {
                    writer.BaseStream.Seek(offset + kvp.Key, SeekOrigin.Begin);
                    writer.Write((UInt32) (offset + fix_block_length + kvp.Value));
                }
            if (_pre == null)
                foreach (var kvp in _postMapping)
                {
                    writer.BaseStream.Seek(0 + kvp.Key, SeekOrigin.Begin);
                    writer.Write((UInt32) (offset + 0 + kvp.Value));
                }

            // set all pre pointers (from pre to post)
            foreach (var kvp in _preMapping)
            {
                writer.BaseStream.Seek(offsets[kvp.Value.Item1] + kvp.Value.Item2, SeekOrigin.Begin);
                writer.Write((UInt32) (offset + kvp.Key));
            }

            // set the most out layer's pre pointers
            if (_pre == null)
                foreach (var kvp in _preMapping2)
                {
                    writer.BaseStream.Seek(offsets[kvp.Value.Item1] + kvp.Value.Item2, SeekOrigin.Begin);
                    writer.Write((UInt32) (0 + kvp.Key));
                }

            // gather global alignments
            if (_post != null)
                foreach (var kvp in _post._globalAlign)
                {
                    align[offset + kvp.Key] = kvp.Value;
                }
            if (_pre == null)
                foreach (var kvp in _globalAlign)
                {
                    align[0 + kvp.Key] = kvp.Value;
                }
            return align;
        }

    };
}
