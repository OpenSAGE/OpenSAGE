using System;
using System.IO;
using OpenSage.Data.Apt;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.Apt.Writer
{
    public static partial class PlainArrayElementWriter
    {
        public static uint GetElementSizeImpl(Export export) =>  Constants.IntPtrSize * 2;
        public static void WriteInAllocatedMemoryImpl(BinaryWriter writer, MemoryPool memory, Export export)
        {
            var exportedItemNameAddress = DataWriter.Write(memory, export.Name);
            writer.Write((UInt32)exportedItemNameAddress);
            writer.Write((UInt32)export.Character);
            return;
        }
    }
}