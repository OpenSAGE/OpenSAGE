using System;
using System.IO;
using OpenSage.Data.Apt;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.Apt.Writer
{
    public static partial class PlainArrayElementWriter
    {
        public static uint GetElementSizeImpl(Import import) => Constants.IntPtrSize * 4;
        public static void WriteInAllocatedMemoryImpl(BinaryWriter writer, MemoryPool memory, Import import)
        {
            var importedMovieNameAddress = DataWriter.Write(memory, import.Movie);
            var importedItemNameAddress = DataWriter.Write(memory, import.Name);
            writer.Write((UInt32)importedMovieNameAddress);
            writer.Write((UInt32)importedItemNameAddress);
            writer.Write((UInt32)import.Character);
            writer.Write((UInt32)0); // pointer
        }
    }
}