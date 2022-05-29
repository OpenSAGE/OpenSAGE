using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace OpenSage.FileFormats
{
    public static class BinaryWriterExtensions2
    {
        // array related

        /// <summary>
        /// Write an array (by giving the writing action and the size) to a stream.
        /// if an element is null, action should return false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writer"></param>
        /// <param name="size"></param>
        /// <param name="action"></param>
        /// <param name="post"></param>
        /// <param name="ptr"></param>
        public static void WriteArrayAtOffset(this BinaryWriter writer, int size, Func<int, BinaryWriter, BinaryMemoryChain, bool> action, BinaryMemoryChain post, bool ptr = false, uint align = 4)
        {
            var curOffset = (UInt32) writer.BaseStream.Position;
            writer.Write((UInt32) 0);
            if (ptr)
                post.WritePointerArrayAtOffset(curOffset, size, action, align);
            else
                post.WriteArrayAtOffset(curOffset, size, action, align);
        }
        public static void WriteArrayAtOffset(this BinaryWriter writer, int size, Action<int, BinaryWriter, BinaryMemoryChain> action, BinaryMemoryChain memory, bool ptr = false, uint align = 4)
        {
            writer.WriteArrayAtOffset(size, (i, w, p) => { action(i, w, p); return true; }, memory, ptr, align);
        }
        public static void WriteArrayAtOffsetWithSize(this BinaryWriter writer, int size, Func<int, BinaryWriter, BinaryMemoryChain, bool> action, BinaryMemoryChain memory, bool ptr = false, uint align = 4)
        {
            writer.Write((UInt32) size);
            writer.WriteArrayAtOffset(size, action, memory, ptr, align);
        }
        public static void WriteArrayAtOffsetWithSize(this BinaryWriter writer, int size, Action<int, BinaryWriter, BinaryMemoryChain> action, BinaryMemoryChain memory, bool ptr = false, uint align = 4)
        {
            writer.Write((UInt32) size);
            writer.WriteArrayAtOffset(size, action, memory, ptr, align);
        }

        public static void WriteArrayAtOffset<T>(this BinaryWriter writer, IList<T> array, BinaryMemoryChain memory, bool ptr = false, uint align = 4) where T : IMemoryStorage
        {
            writer.WriteArrayAtOffset(array.Count, (i, w, p) => array[i].Write(w, p), memory, ptr, align);
        }
        public static void WriteArrayAtOffsetWithSize<T>(this BinaryWriter writer, IList<T> array, BinaryMemoryChain memory, bool ptr = false, uint align = 4) where T : IMemoryStorage
        {
            writer.WriteArrayAtOffsetWithSize(array.Count, (i, w, p) => array[i].Write(w, p), memory, ptr, align);
        }

        // alignment related

        public static long AlignBy(this long curPos, uint c)
        {
            var a = c - (curPos % c);
            return curPos + (a >= c ? 0 : a);
        }
        public static uint AlignBy(this uint curPos, uint c)
        {
            var a = c - (curPos % c);
            return curPos + (a >= c ? 0 : a);
        }

        public static uint Align(this BinaryWriter writer, uint c, BinaryMemoryChain postChain = null)
        {
            uint a = c - (uint) (writer.BaseStream.Position % c);
            a = a >= c ? 0 : a;
            if (a > 0x7fffffff)
            {
                writer.Seek(0x7fffffff, SeekOrigin.Current);
                writer.Seek((int) (a - 0x7fffffff), SeekOrigin.Current);
            }
            else
                writer.Seek((int) a, SeekOrigin.Current);
            if (postChain != null)
                postChain.AlignPre(c);
            return a;
        }

        // others

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="memory"></param>
        /// <param name="startOffset">the offset when data inside the memory chain starts.</param>
        public static void DumpMemoryChain(this BinaryWriter writer, BinaryMemoryChain memory, long startOffset = -1)
        {
            uint so = 0;
            if (startOffset < 0)
                so = (uint) writer.BaseStream.Position;
            else
                so = (uint) startOffset;

            var aligns = memory.SerializeAndGatherAlignment(writer, so);
            if (aligns.Count == 0)
                return;

            var newMemory = new BinaryMemoryChain();
            var curPos = writer.BaseStream.Length;
            foreach (var (pointerPos, writerFunc) in aligns)
            {
                writer.Seek((int) curPos, SeekOrigin.Begin);
                var (startPos, endPos) = writerFunc(writer, newMemory);
                writer.Seek((int) pointerPos, SeekOrigin.Begin);
                writer.Write((UInt32) startPos);
                curPos = endPos;
            }
            DumpMemoryChain(writer, newMemory, curPos);
        }

        public static void WriteStringAtOffset(this BinaryWriter writer, string value, BinaryMemoryChain memory)
        {
            var cur_offset = (uint) writer.BaseStream.Position;
            writer.Write((UInt32) 0); // keep space of the address
            memory.WriteStringAtOffset(cur_offset, value);
        }

        public static void Write(Func<BinaryWriter, BinaryMemoryChain, long> write, Func<Stream> streamGetter)
        {
            using var stream = streamGetter();
            using var writer = new BinaryWriter(stream);
            using var memory = new BinaryMemoryChain();
            var offset = write(writer, memory);
            writer.DumpMemoryChain(memory, offset);

        }
    }
}
