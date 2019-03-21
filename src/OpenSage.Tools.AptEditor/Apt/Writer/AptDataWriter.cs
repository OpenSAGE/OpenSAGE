using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.Apt.Writer
{
    public sealed class AptDataAsBytes
    {
        public byte[] Data;
        public uint EntryOffset;

        public AptDataAsBytes(byte[] data, uint entryOffset)
        {
            Data = data;
            EntryOffset = entryOffset;
        }
    }

    public static class AptDataWriter
    {
        public static AptDataAsBytes Write(Movie movie)
        {
            var memoryPool = new MemoryPool();
            memoryPool.WriteDataToNewChunk(Encoding.ASCII.GetBytes(Constants.AptFileMagic));
            var address = DataWriter.Write(memoryPool, movie);
            return new AptDataAsBytes(memoryPool.GetMemoryStream().ToArray(), (uint)address);
        }
    };

    public static partial class DataWriter
    {
        public static BinaryWriter GetWriter(MemoryChunk chunk)
        {
            var stream = new MemoryStream(chunk.Memory, true);
            return new BinaryWriter(stream);
        }

        public static uint Write<T>(MemoryPool memory, T item)
        {
            return WriteImpl(memory, (dynamic)item);
        }

        static uint WriteImpl(MemoryPool memory, object fallback)
        {
            throw new NotImplementedException("FrameItemWriter fallback: " + fallback.GetType());
        }
    }
}