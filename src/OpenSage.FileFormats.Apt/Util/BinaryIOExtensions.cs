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

        public static void WriteStringAtOffset(this BinaryWriter writer, string value, MemoryPool memory)
        {
            var cur_offset = (uint) writer.BaseStream.Position;
            writer.Write((UInt32) 0); // keep space of the address
            memory.WriteStringAtOffset(cur_offset, value);
        }
        /// <summary>
        /// Write an array (by giving the writing action and the size) to a stream.
        /// if an element is null, action should return false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writer"></param>
        /// <param name="size"></param>
        /// <param name="action"></param>
        /// <param name="memory"></param>
        /// <param name="ptr"></param>
        public static void WriteArrayAtOffset(this BinaryWriter writer, int size, Func<int, BinaryWriter, MemoryPool, bool> action, MemoryPool memory, bool ptr = false)
        {
            var cur_offset = (UInt32) writer.BaseStream.Position;
            writer.Write((UInt32) 0);
            if (ptr)
                memory.WritePointerArrayAtOffset(cur_offset, size, action);
            else
                memory.WriteArrayAtOffset(cur_offset, size, action);
        }
        public static void WriteArrayAtOffset(this BinaryWriter writer, int size, Action<int, BinaryWriter, MemoryPool> action, MemoryPool memory, bool ptr = false)
        {
            writer.WriteArrayAtOffset(size, (i, w, p) => { action(i, w, p); return true; }, memory, ptr);
        }
        public static void WriteArrayAtOffsetWithSize(this BinaryWriter writer, int size, Func<int, BinaryWriter, MemoryPool, bool> action, MemoryPool memory, bool ptr = false)
        {
            writer.Write((UInt32) size);
            writer.WriteArrayAtOffset(size, action, memory, ptr);
        }
        public static void WriteArrayAtOffsetWithSize(this BinaryWriter writer, int size, Action<int, BinaryWriter, MemoryPool> action, MemoryPool memory, bool ptr = false)
        {
            writer.Write((UInt32) size);
            writer.WriteArrayAtOffset(size, action, memory, ptr);
        }

        public static void WriteArrayAtOffset<T>(this BinaryWriter writer, IList<T> array, MemoryPool memory, bool ptr = false) where T : IDataStorage
        {
            writer.WriteArrayAtOffset(array.Count, (i, w, p) => array[i].Write(w, p), memory, ptr);
        }
        public static void WriteArrayAtOffsetWithSize<T>(this BinaryWriter writer, IList<T> array, MemoryPool memory, bool ptr = false) where T : IDataStorage
        {
            writer.WriteArrayAtOffsetWithSize(array.Count, (i, w, p) => array[i].Write(w, p), memory, ptr);
        }


        public static void WriteInstructions(this BinaryWriter writer, InstructionStorage insts, MemoryPool memory)
        {
            // memory.RegisterPostOffset((uint) writer.BaseStream.Position);
            // writer.Write((UInt32) 0);
            // insts.Write(memory.Writer, memory.Post);

            memory.RegisterGlobalAlignObject((uint) writer.BaseStream.Position, (w, p) => {
                w.Seek((4 - (int) (w.BaseStream.Position % 4)) % 4, SeekOrigin.Current);
                uint ret1 = (uint) w.BaseStream.Position;
                insts.Write(w, p);
                uint ret2 = (uint) w.BaseStream.Position;
                return (ret1, ret2);
            });
            writer.Write((UInt32) 0);
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
