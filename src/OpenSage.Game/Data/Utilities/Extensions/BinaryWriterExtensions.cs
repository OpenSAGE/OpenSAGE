using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace OpenSage.Data.Utilities.Extensions
{
    internal static class BinaryWriterExtensions
    {
        public static void WriteUInt24(this BinaryWriter writer, uint value)
        {
            for (var i = 0; i < 3; i++)
            {
                writer.Write((byte) ((value >> (i * 8)) & 0xFF));
            }
        }

        public static void WriteUInt16PrefixedAsciiString(this BinaryWriter writer, string value)
        {
            if (value.Length > ushort.MaxValue)
            {
                throw new ArgumentException();
            }

            writer.Write((ushort) value.Length);

            writer.Write(BinaryUtility.AnsiEncoding.GetBytes(value));
        }

        public static void WriteUInt16PrefixedUnicodeString(this BinaryWriter writer, string value)
        {
            if (value.Length > ushort.MaxValue)
            {
                throw new ArgumentException();
            }

            writer.Write((ushort) value.Length);

            writer.Write(Encoding.Unicode.GetBytes(value));
        }

        public static void WriteUInt16Array2D(this BinaryWriter writer, ushort[,] values)
        {
            var width = values.GetLength(0);
            var height = values.GetLength(1);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    writer.Write(values[x, y]);
                }
            }
        }

        public static void WriteSingleBitBooleanArray2D(this BinaryWriter writer, bool[,] values)
        {
            var width = values.GetLength(0);
            var height = values.GetLength(1);

            for (var y = 0; y < height; y++)
            {
                byte value = 0;
                for (var x = 0; x < width; x++)
                {
                    if (x > 0 && x % 8 == 0)
                    {
                        writer.Write(value);
                        value = 0;
                    }

                    var boolValue = values[x, y];

                    value |= (byte) ((boolValue ? 0 : 1) << (x % 8));
                }

                // Write last value.
                writer.Write(value);
            }
        }

        public static void Write(this BinaryWriter writer, Vector2 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public static void Write(this BinaryWriter writer, Vector3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }
    }
}
