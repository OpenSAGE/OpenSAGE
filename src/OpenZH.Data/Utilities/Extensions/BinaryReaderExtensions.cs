using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenZH.Data.Utilities.Extensions
{
    internal static class BinaryReaderExtensions
    {
        public static uint ReadBigEndianUInt32(this BinaryReader reader)
        {
            var array = reader.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array);
            return BitConverter.ToUInt32(array, 0);
        }

        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            var sb = new StringBuilder();

            char c;
            while ((c = reader.ReadChar()) != '\0')
            {
                sb.Append(c);
            }

            return sb.ToString();
        }

        public static T ReadStruct<T>(this BinaryReader reader)
            where T : struct
        {
            var bytes = reader.ReadBytes(Marshal.SizeOf<T>());

            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var theStructure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();

            return theStructure;
        }

        public static string ReadFixedLengthString(this BinaryReader reader, int count)
        {
            var chars = reader.ReadChars(count);
            return new string(chars).TrimEnd('\0');
        }
    }
}
