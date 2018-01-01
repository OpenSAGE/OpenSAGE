using System;
using System.IO;
using System.Numerics;
using System.Text;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Data.Utilities.Extensions
{
    internal static class BinaryWriterExtensions
    {
        public static void WriteBooleanUInt32(this BinaryWriter writer, bool value)
        {
            writer.Write(value);
            writer.Write((byte) 0);
            writer.Write((byte) 0);
            writer.Write((byte) 0);
        }

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

        public static void WriteByteArray2DAsEnum<TEnum>(this BinaryWriter writer, TEnum[,] values)
            where TEnum : struct
        {
            var width = values.GetLength(0);
            var height = values.GetLength(1);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    writer.Write(Convert.ToByte(values[x, y]));
                }
            }
        }

        public static void WriteSingleBitBooleanArray2D(this BinaryWriter writer, bool[,] values, byte padValue = 0x0)
        {
            var width = values.GetLength(0);
            var height = values.GetLength(1);

            for (var y = 0; y < height; y++)
            {
                byte value = width < 8 ? padValue : (byte) 0;
                for (var x = 0; x < width; x++)
                {
                    if (x > 0 && x % 8 == 0)
                    {
                        writer.Write(value);
                        value = (x > width - 8) ? padValue : (byte) 0;
                    }

                    var boolValue = values[x, y];

                    value |= (byte) ((boolValue ? 1 : 0) << (x % 8));
                }

                // Write last value.
                writer.Write(value);
            }
        }

        public static void Write(this BinaryWriter writer, in Vector2 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public static void Write(this BinaryWriter writer, in Vector3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        public static void Write(this BinaryWriter writer, in Quaternion value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        public static void Write(this BinaryWriter writer, in MapLine2D value)
        {
            writer.Write(value.V0);
            writer.Write(value.V1);
        }

        public static void Write(this BinaryWriter writer, in ColorRgb value)
        {
            writer.Write(value.R);
            writer.Write(value.G);
            writer.Write(value.B);
        }

        public static void Write(this BinaryWriter writer, in ColorRgbF value)
        {
            writer.Write(value.R);
            writer.Write(value.G);
            writer.Write(value.B);
        }

        public static void WriteFourCc(this BinaryWriter writer, string fourCc, bool bigEndian = false)
        {
            if (string.IsNullOrEmpty(fourCc) || fourCc.Length != 4)
            {
                throw new ArgumentException();
            }

            if (bigEndian)
            {
                writer.Write(fourCc[3]);
                writer.Write(fourCc[2]);
                writer.Write(fourCc[1]);
                writer.Write(fourCc[0]);
            }
            else
            {
                writer.Write(fourCc[0]);
                writer.Write(fourCc[1]);
                writer.Write(fourCc[2]);
                writer.Write(fourCc[3]);
            }
        }
    }
}
