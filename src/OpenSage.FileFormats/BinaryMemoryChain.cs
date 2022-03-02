﻿using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace OpenSage.FileFormats
{
    public sealed class BinaryMemoryChain: IDisposable
    {
        public const uint IntPtrSize = 4;

        public delegate (uint, uint) AlignedObjectWriter(BinaryWriter writer, BinaryMemoryChain memory);

        private readonly Dictionary<uint, (BinaryMemoryChain, uint)> _preMapping;
        private readonly Dictionary<uint, (BinaryMemoryChain, uint)> _preMapping2;
        private readonly Dictionary<uint, AlignedObjectWriter> _globalAlign;
        private Dictionary<uint, uint> _postMapping;
        private MemoryStream _stream;
        private BinaryWriter _writer;

        public uint Alignment { get; private set; }

        public BinaryWriter Writer => _writer;

        private BinaryMemoryChain _post = null; 
        private BinaryMemoryChain _pre = null;

        private BinaryMemoryChain _postAutoCreate
        {
            get
            {
                if (_post == null)
                    _post = new(this);
                return _post;
            }
        }

        public BinaryMemoryChain Pre => _pre;
        public BinaryMemoryChain Post => _postAutoCreate;

        public BinaryMemoryChain() : this(null) { }

        public BinaryMemoryChain(BinaryMemoryChain pre) 
        {
            _pre = pre;
            _preMapping = new();
            _preMapping2 = _pre == null ? new() : _pre._preMapping;
            _globalAlign = new();
            _postMapping = new();
            _stream = new();
            _writer = new(_stream); // direct alignment is not recommended
            Alignment = 1;
        }

        public void Dispose()
        {
            if (_post != null)
                _post.Dispose();
            _writer.Dispose();
            _stream.Dispose();
        }

        static Dictionary<(int, int), int> GcdRec = new();
        static Dictionary<(int, int), int> GcmRec = new();
        static int GCD(int a, int b)
        {
            if (a < b)
            {
                a = a + b;
                b = a - b;
                a = a - b;
            }
            if (GcdRec.TryGetValue((a, b), out var c))
                return c;
            var ans = (a % b == 0) ? b : GCD(a % b, b);
            GcdRec[(a, b)] = ans;
            return ans;
        }

        static int GCM(int a, int b)
        {
            if (GcmRec.TryGetValue((a, b), out var c))
                return c;
            var ans = a * b / GCD(a, b);
            GcmRec[(a, b)] = ans;
            return ans;
        }

        public bool Align(uint c)
        {
            var ans = c > 0;
            if (ans)
                Alignment = (uint) GCM((int) Alignment, (int) c);
            _writer.Align(c);
            return ans;
        }

        public bool Align(int c)
        {
            var ans = c > 0;
            if (ans)
                Alignment = (uint) GCM((int) Alignment, c);
            _writer.Align((uint) c);
            return ans;
        }

        public bool AlignPre(uint c) { return _pre == null ? false : _pre.Align(c); }
        public bool AlignPost(uint c) { return _postAutoCreate.Align(c); }
        public bool AlignPre(int c) { return _pre == null ? false : _pre.Align(c); }
        public bool AlignPost(int c) { return _postAutoCreate.Align(c); }

        /// <summary>
        /// (this, cur_offset) -> (target, offset)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public uint RegisterPreOffset(BinaryMemoryChain target, uint offset, uint align = 1)
        {
            Align(align);
            var ret = (uint) _stream.Position;
            var pm = target == null ? _preMapping2 : target._preMapping;
            pm[offset] = (this, ret);
            return ret;
        }

        /*
        public (BinaryMemoryChain, uint) RemovePreOffset(BinaryMemoryChain target, uint offset)
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
        public uint RegisterGlobalAlignObject(uint offset, AlignedObjectWriter objWrite, uint align = 1)
        {
            Align(align);
            var ret = (uint) _stream.Position;
            _globalAlign[offset] = objWrite;
            return ret;
        }

        /// <summary>
        /// (pre, offset) -> (this, reg_offset)
        /// reg_offset is assigned to _stream.Position if negative
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="regOffset"></param>
        /// <returns></returns>
        public uint RegisterPostOffset(uint offset, long regOffset = -1, uint align = 1)
        {
            Align(align);
            if (regOffset < 0)
                regOffset = _stream.Position;
            else
                regOffset = regOffset.AlignBy(align);
            _postMapping[offset] = (uint) regOffset;
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

        public void WriteArrayAtOffset(uint offset, int length, Func<int, BinaryWriter, BinaryMemoryChain, bool> action, uint align = 4)
        {
            Align(align);
            RegisterPostOffset(offset);
            for (int i = 0; i < length; ++i)
            {
                var flag = action(i, _writer, _postAutoCreate);
                // if flag == false:
                // The design is that action handles it
            }
        }

        public void WritePointerArrayAtOffset(uint offset, int length, Func<int, BinaryWriter, BinaryMemoryChain, bool> action, uint align = 4)
        {
            Align(IntPtrSize); // since the pointers are 4 bytes long
            RegisterPostOffset(offset);
            for (int i = 0; i < length; ++i)
            {
                var curOffset = (uint) _stream.Position;
                var post = _postAutoCreate;
                post.RegisterPostOffset(curOffset, align : align);
                var flag = action(i, post._writer, post._postAutoCreate);
                if (!flag)
                    post.RemovePostOffsetAndSeek(curOffset);
                _writer.Write((UInt32) 0);
            }
        }

        /// <summary>
        /// STRONGLY NOT RECOMMENDED TO CALL although it is public. 
        /// Use BinaryIOExtensions.DumpMemoryChain Instead. 
        /// </summary>
        /// <param name="writer">the writer of the most outer layer (usually a file writer).</param>
        /// <param name="offset">the offset from file start to chain start.</param>
        /// <param name="offsets"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        public Dictionary<uint, AlignedObjectWriter> SerializeAndGatherAlignment(
            BinaryWriter writer,
            uint offsetRaw = 0,
            Dictionary<BinaryMemoryChain, uint> offsets = null,
            Dictionary<uint, AlignedObjectWriter> align = null
            )
        {
            // align offset
            var offsetAligned = offsetRaw.AlignBy(Alignment);
            Console.WriteLine($"{Alignment}, {offsetRaw}, {offsetRaw % Alignment}, {offsetAligned}, {offsetAligned % Alignment}");

            // init align
            if (align == null)
                align = new();

            var curBlockLength = (uint) _stream.Length;
            if (curBlockLength == 0) return align;
            var curBlockLengthAligned = (offsetAligned + curBlockLength).AlignBy(_post != null ? _post.Alignment : 1);

            // init offset
            if (offsets == null)
                offsets = new();
            offsets[this] = offsetAligned;

            // the last chain muse be serialized first
            if (_post != null)
                _post.SerializeAndGatherAlignment(writer, offsetAligned + curBlockLength, offsets, align);

            // copy anything from the data chunk to the file
            _stream.Seek(0, SeekOrigin.Begin);
            writer.BaseStream.Seek(offsetAligned, SeekOrigin.Begin);
            _stream.CopyTo(writer.BaseStream);
            _stream.Seek(curBlockLength, SeekOrigin.Begin);

            // set all post pointers to correct positions
            if (_post != null)
                foreach (var kvp in _post._postMapping) // this[key] -> _post[val]
                {
                    writer.BaseStream.Seek(offsetAligned + kvp.Key, SeekOrigin.Begin);
                    writer.Write((UInt32) (curBlockLengthAligned + kvp.Value));
                }
            if (_pre == null)
                foreach (var kvp in _postMapping)
                {
                    writer.BaseStream.Seek(0 + kvp.Key, SeekOrigin.Begin);
                    writer.Write((UInt32) (offsetAligned + 0 + kvp.Value));
                }

            // set all pre pointers (from pre to post)
            foreach (var kvp in _preMapping)
            {
                writer.BaseStream.Seek(offsets[kvp.Value.Item1] + kvp.Value.Item2, SeekOrigin.Begin);
                writer.Write((UInt32) (offsetAligned + kvp.Key));
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
                    align[offsetAligned + kvp.Key] = kvp.Value;
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