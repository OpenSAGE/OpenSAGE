using System;
using System.IO;
using System.Numerics;
using OpenSage.FileFormats;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.Apt.Writer
{
    public static partial class PlainArrayElementWriter
    {
        public static uint GetElementSizeImpl(UInt32 n) => 4;
        public static void WriteInAllocatedMemoryImpl(BinaryWriter writer, MemoryPool memory, UInt32 n) => writer.Write(n);

        public static uint GetElementSizeImpl(Vector2 v) => 4 * 2;
        public static void WriteInAllocatedMemoryImpl(BinaryWriter writer, MemoryPool memory, Vector2 v) => writer.Write(v);

        public static uint GetElementSizeImpl(IndexedTriangle t) => 2 * 3;
        public static void WriteInAllocatedMemoryImpl(BinaryWriter writer, MemoryPool memory, IndexedTriangle t)
        {
            writer.Write((UInt16)t.IDX0);
            writer.Write((UInt16)t.IDX1);
            writer.Write((UInt16)t.IDX2);
        }
    }
}