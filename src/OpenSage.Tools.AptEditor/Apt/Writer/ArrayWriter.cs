using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.Apt.Writer
{
    public static partial class PlainArrayElementWriter
    {
        public static uint GetElementSize<T>(T element)
        {
            return GetElementSizeImpl((dynamic)element);
        }
        public static void WriteInAllocatedMemory<T>(BinaryWriter writer, MemoryPool memory, T element)
        {
            WriteInAllocatedMemoryImpl(writer, memory, (dynamic)element);
        }

        private static uint GetElementSizeImpl(object fallback)
        {
            throw new NotImplementedException("PlainArrayElementWriter GetElementSize fallback: " + fallback.GetType());
        }

        private static void WriteInAllocatedMemoryImpl(BinaryWriter writer, MemoryPool memory, object fallback)
        {
            throw new NotImplementedException("PlainArrayElementWriter WriteInAllocatedMemory fallback: " + fallback.GetType());
        }
    }

    public static class ArrayWriter
    {
        // returns: array start address
        static public uint WritePlainArray<T>(MemoryPool memory, ICollection<T> list)
        {
            memory.AllocateBytesForPadding(Constants.IntPtrSize);
            if(list.Count == 0)
            {
                return memory.Allocate(0).StartAddress;
            }

            var arrayMemory = memory.Allocate((uint)list.Count * PlainArrayElementWriter.GetElementSize(list.First()));
            using(var stream = new MemoryStream(arrayMemory.Memory, true))
            using(var writer = new BinaryWriter(stream))
            {
                foreach(var item in list)
                {
                    PlainArrayElementWriter.WriteInAllocatedMemory(writer, memory, item);
                }

                if(stream.Position != arrayMemory.Memory.Length)
                {
                    throw new InvalidDataException("Unexpected array size!");
                }
            }

            return arrayMemory.StartAddress;
        }

        public static uint WriteArrayOfPointers<T>(MemoryPool memory, ICollection<T> list)
        {
            memory.AllocateBytesForPadding(Constants.IntPtrSize);
            var arrayChunk = memory.Allocate((uint)list.Count * Constants.IntPtrSize);
            using (var arrayStream = new MemoryStream(arrayChunk.Memory, true))
            using (var arrayWriter = new BinaryWriter(arrayStream))
            {
                foreach(var item in list)
                {
                    if(item == null)
                    {
                        arrayWriter.Write((UInt32)0);
                    }

                    memory.AllocateBytesForPadding(Constants.IntPtrSize);
                    var address = DataWriter.Write(memory, item);

                    arrayWriter.Write((UInt32)address);
                }
            }
            
            return arrayChunk.StartAddress;
        }
    }
}
