using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using OpenSage.Mathematics;
using OpenSage.FileFormats.Apt.ActionScript;

namespace OpenSage.FileFormats.Apt
{
    public static class BinaryIOExtensions
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
        public static void WriteArrayAtOffset(this BinaryWriter writer, int size, Func<int, BinaryWriter, MemoryPool, bool> action, MemoryPool post, bool ptr = false, uint align = 4)
        {
            var curOffset = (UInt32) writer.BaseStream.Position;
            writer.Write((UInt32) 0);
            if (ptr)
                post.WritePointerArrayAtOffset(curOffset, size, action, align);
            else
                post.WriteArrayAtOffset(curOffset, size, action, align);
        }
        public static void WriteArrayAtOffset(this BinaryWriter writer, int size, Action<int, BinaryWriter, MemoryPool> action, MemoryPool memory, bool ptr = false, uint align = 4)
        {
            writer.WriteArrayAtOffset(size, (i, w, p) => { action(i, w, p); return true; }, memory, ptr, align);
        }
        public static void WriteArrayAtOffsetWithSize(this BinaryWriter writer, int size, Func<int, BinaryWriter, MemoryPool, bool> action, MemoryPool memory, bool ptr = false, uint align = 4)
        {
            writer.Write((UInt32) size);
            writer.WriteArrayAtOffset(size, action, memory, ptr, align);
        }
        public static void WriteArrayAtOffsetWithSize(this BinaryWriter writer, int size, Action<int, BinaryWriter, MemoryPool> action, MemoryPool memory, bool ptr = false, uint align = 4)
        {
            writer.Write((UInt32) size);
            writer.WriteArrayAtOffset(size, action, memory, ptr, align);
        }

        public static void WriteArrayAtOffset<T>(this BinaryWriter writer, IList<T> array, MemoryPool memory, bool ptr = false, uint align = 4) where T : IDataStorage
        {
            writer.WriteArrayAtOffset(array.Count, (i, w, p) => array[i].Write(w, p), memory, ptr, align);
        }
        public static void WriteArrayAtOffsetWithSize<T>(this BinaryWriter writer, IList<T> array, MemoryPool memory, bool ptr = false, uint align = 4) where T : IDataStorage
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

        public static uint Align(this BinaryWriter writer, uint c, MemoryPool postPool = null)
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
            if (postPool != null)
                postPool.AlignPre(c);
            return a;
        }

        // others

        public static void WriteInstructions(this BinaryWriter writer, InstructionStorage insts, MemoryPool memory)
        {
            memory.RegisterPostOffset((uint) writer.BaseStream.Position, align: Constants.IntPtrSize);
            writer.Write((UInt32) 0);
            insts.Write(memory.Writer, memory.Post);
            /*
            memory.RegisterGlobalAlignObject((uint) writer.BaseStream.Position, (curWriter, postPool) =>
            {
                curWriter.Align(Constants.IntPtrSize, postPool: postPool);
                uint ret1 = (uint) curWriter.BaseStream.Position;
                insts.Write(curWriter, postPool);
                uint ret2 = (uint) curWriter.BaseStream.Position;
                return (ret1, ret2);
            });
            writer.Write((UInt32) 0); // int pointer
            */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="pool"></param>
        /// <param name="startOffset">the offset when data inside the pool starts.</param>
        public static void DumpMemoryPool(this BinaryWriter writer, MemoryPool pool, long startOffset = -1)
        {
            uint so = 0;
            if (startOffset < 0)
                so = (uint) writer.BaseStream.Position;
            else
                so = (uint) startOffset;

            var aligns = pool.SerializeAndGatherAlignment(writer, so);
            if (aligns.Count == 0)
                return;
            
            var newPool = new MemoryPool();
            var curPos = writer.BaseStream.Length;
            foreach (var (pointerPos, writerFunc) in aligns)
            {
                writer.Seek((int) curPos, SeekOrigin.Begin);
                var (startPos, endPos) = writerFunc(writer, newPool);
                writer.Seek((int) pointerPos, SeekOrigin.Begin);
                writer.Write((UInt32) startPos);
                curPos = endPos;
            }
            DumpMemoryPool(writer, newPool, curPos);
        }

        public static void WriteStringAtOffset(this BinaryWriter writer, string value, MemoryPool memory)
        {
            var cur_offset = (uint) writer.BaseStream.Position;
            writer.Write((UInt32) 0); // keep space of the address
            memory.WriteStringAtOffset(cur_offset, value);
        }

        public static void Write(Func<BinaryWriter, MemoryPool, long> write, Func<Stream> streamGetter)
        {
            using var stream = streamGetter();
            using var writer = new BinaryWriter(stream);
            using var pool = new MemoryPool();
            var offset = write(writer, pool);
            writer.DumpMemoryPool(pool, offset);

        }
    }
}
