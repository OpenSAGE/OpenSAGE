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

        public static string ReadUInt16PrefixedString(this BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            return Encoding.ASCII.GetString(reader.ReadBytes(length));
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

        public static ushort[] ReadUInt16Array(this BinaryReader reader, uint length)
        {
            var result = new ushort[length];

            for (var i = 0; i < length; i++)
            {
                result[i] = reader.ReadUInt16();
            }

            return result;
        }

        public static bool[] ReadSingleBitBooleanArray(this BinaryReader reader, uint length)
        {
            var result = new bool[length];

            var readBytes = 0;

            var temp = (byte) 0;
            for (var i = 0; i < length; i++)
            {
                if (i % 8 == 0)
                {
                    temp = reader.ReadByte();
                    readBytes++;
                }
                result[i] = (temp & (1 << (i % 8))) > 0;
            }

            // Read up to next 4-byte boundary?
            var bytesToRead = 4 - (readBytes % 4);
            if (bytesToRead != 4)
            {
                for (var i = 0; i < bytesToRead; i++)
                {
                    var value = reader.ReadByte();
                    if (value != 0)
                    {
                        throw new InvalidDataException();
                    }
                }
            }

            return result;
        }        
    }
}
