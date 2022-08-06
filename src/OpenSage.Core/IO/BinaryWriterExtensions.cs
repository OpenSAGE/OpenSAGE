using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenSage.FileFormats
{
    public static class BinaryWriterExtensions
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

        public static void WriteBytePrefixedAsciiString(this BinaryWriter writer, string value)
        {
            if (value.Length > byte.MaxValue)
            {
                throw new ArgumentException();
            }

            writer.Write((byte) value.Length);

            writer.Write(BinaryUtility.AnsiEncoding.GetBytes(value));
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

        public static void WriteBytePrefixedUnicodeString(this BinaryWriter writer, string value)
        {
            if (value.Length > byte.MaxValue)
            {
                throw new ArgumentException();
            }

            writer.Write((byte)value.Length);

            writer.Write(Encoding.Unicode.GetBytes(value));
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

        public static void WriteFixedLengthString(this BinaryWriter writer, string value, int count)
        {
            writer.Write(value.ToCharArray());
            writer.Write('\0');

            for (var i = value.Length + 1; i < count; i++)
            {
                writer.Write((char) 0);
            }
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

        public static void WriteUIntArray2D(this BinaryWriter writer, uint[,] values, uint bitSize)
        {
            var width = values.GetLength(0);
            var height = values.GetLength(1);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = values[x, y];
                    switch (bitSize)
                    {
                        case 16:
                            writer.Write((ushort) value);
                            break;

                        case 32:
                            writer.Write(value);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(bitSize));
                    }
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

        public static void WriteByteArray2D(this BinaryWriter writer, byte[,] values)
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

        public static void WriteSingleBitBooleanArray(this BinaryWriter writer, bool[] values)
        {
            var length = values.Length;
            byte value = 0;
            for (var i = 0; i < length; i++)
            {
                if (i > 0 && i % 8 == 0)
                {
                    writer.Write(value);
                    value = 0;
                }

                var boolValue = values[i];

                value |= (byte) ((boolValue ? 1 : 0) << (i % 8));
            }

            // Write last value.
            writer.Write(value);
        }

        public static void WriteEnumAsUInt32<TEnum>(this BinaryWriter writer, TEnum value)
            where TEnum : struct
        {
            var uintValue = Unsafe.As<TEnum, uint>(ref value);
            writer.Write(uintValue);
        }

        public static void WriteEnumAsByte<TEnum>(this BinaryWriter writer, TEnum value)
            where TEnum : struct
        {
            var byteValue = Unsafe.As<TEnum, byte>(ref value);
            writer.Write(byteValue);
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

        public static void Write(this BinaryWriter writer, in Vector4 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        public static void Write(this BinaryWriter writer, in Quaternion value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        public static void Write(this BinaryWriter writer, DateTime value)
        {
            writer.Write((ushort)value.Year);
            writer.Write((ushort)value.Month);
            writer.Write((ushort)value.DayOfWeek);
            writer.Write((ushort)value.Hour);
            writer.Write((ushort)value.Minute);
            writer.Write((ushort)value.Second);
            writer.Write((ushort)value.Millisecond);
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

        public static void WriteNullTerminatedString(this BinaryWriter writer, in string content)
        {
            foreach(var b in Encoding.UTF8.GetBytes(content))
            {
                writer.Write(b);
            }

            writer.Write('\0');
        }

        public static void WriteBigEndianUInt32(this BinaryWriter writer, uint num)
        {
            var array = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array);
            writer.Write(array);
        }
    }
}
