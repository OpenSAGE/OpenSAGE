using System;
using System.IO;
using System.Numerics;
using System.Text;
using OpenSage.Mathematics;
using System.Linq;
using System.Collections.Generic;
using OpenSage.Data.Map;

namespace OpenSage.Data.Utilities.Extensions
{
    internal static class BinaryReaderExtensions
    {
        public static bool ReadBooleanChecked(this BinaryReader reader)
        {
            var value = reader.ReadByte();

            switch (value)
            {
                case 0:
                    return false;

                case 1:
                    return true;

                default:
                    throw new InvalidDataException();
            }
        }

        public static bool ReadBooleanUInt32Checked(this BinaryReader reader)
        {
            var result = reader.ReadBooleanChecked();

            var unused = reader.ReadUInt24();
            if (unused != 0)
            {
                throw new InvalidDataException();
            }

            return result;
        }

        public static uint ReadBigEndianUInt32(this BinaryReader reader)
        {
            var array = reader.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array);
            return BitConverter.ToUInt32(array, 0);
        }

        public static uint ReadUInt24(this BinaryReader reader)
        {
            var result = 0u;
            for (var i = 0; i < 3; i++)
            {
                result |= ((uint) reader.ReadByte() << (i * 8));
            }
            return result;
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

        public static string ReadUInt16PrefixedAsciiString(this BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            return BinaryUtility.AnsiEncoding.GetString(reader.ReadBytes(length));
        }

        public static string ReadUInt32PrefixedAsciiString(this BinaryReader reader)
        {
            var length = reader.ReadUInt32();
            return BinaryUtility.AnsiEncoding.GetString(reader.ReadBytes((int) length));
        }

        public static string ReadUInt16PrefixedUnicodeString(this BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            return Encoding.Unicode.GetString(reader.ReadBytes(length * 2));
        }

        public static string ReadUInt32PrefixedNegatedUnicodeString(this BinaryReader reader)
        {
            var length = reader.ReadUInt32();
            var bytes = reader.ReadBytes((int) length * 2);
            var negatedBytes = new byte[bytes.Length];
            for (var i = 0; i < negatedBytes.Length; i++)
            {
                negatedBytes[i] = (byte) ~bytes[i];
            }
            return Encoding.Unicode.GetString(negatedBytes);
        }

        public static string ReadFixedLengthString(this BinaryReader reader, int count)
        {
            if (count == 0)
            {
                return string.Empty;
            }

            var chars = reader.ReadChars(count);

            var result = new string(chars);

            // There might be garbage after the \0 character, so we can't just do TrimEnd('\0').
            if (result.Contains('\0'))
                return result.Substring(0, result.IndexOf('\0'));
            else
                return result;
        }

        public static ushort[,] ReadUInt16Array2D(this BinaryReader reader, uint width, uint height)
        {
            var result = new ushort[width, height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    result[x, y] = reader.ReadUInt16();
                }
            }

            return result;
        }

        public static TEnum[,] ReadByteArray2DAsEnum<TEnum>(this BinaryReader reader, uint width, uint height)
            where TEnum : struct
        {
            var result = new TEnum[width, height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    result[x, y] = reader.ReadByteAsEnum<TEnum>();
                }
            }

            return result;
        }

        public static byte[,] ReadByteArray2D(this BinaryReader reader, uint width, uint height)
        {
            var result = new byte[width, height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    result[x, y] = reader.ReadByte();
                }
            }

            return result;
        }

        public static bool[,] ReadSingleBitBooleanArray2D(this BinaryReader reader, uint width, uint height)
        {
            var result = new bool[width, height];

            for (var y = 0; y < height; y++)
            {
                var temp = (byte) 0;
                for (var x = 0; x < width; x++)
                {
                    if (x % 8 == 0)
                    {
                        temp = reader.ReadByte();
                    }
                    result[x, y] = (temp & (1 << (x % 8))) != 0;
                }
            }

            return result;
        }

        public static bool[] ReadSingleBitBooleanArray(this BinaryReader reader, uint count)
        {
            var result = new bool[count];

            var temp = (byte) 0;
            for (var i = 0; i < count; i++)
            {
                if (i % 8 == 0)
                {
                    temp = reader.ReadByte();
                }
                result[i] = (temp & (1 << (i % 8))) != 0;
            }

            return result;
        }

        public static TEnum ReadUInt32AsEnum<TEnum>(this BinaryReader reader)
            where TEnum : struct
        {
            var value = reader.ReadUInt32();

            return EnumUtility.CastValueAsEnum<uint, TEnum>(value);
        }

        public static TEnum ReadUInt16AsEnum<TEnum>(this BinaryReader reader)
            where TEnum : struct
        {
            var value = reader.ReadUInt16();

            return EnumUtility.CastValueAsEnum<ushort, TEnum>(value);
        }

        public static TEnum ReadInt32AsEnum<TEnum>(this BinaryReader reader)
           where TEnum : struct
        {
            var value = reader.ReadInt32();

            return EnumUtility.CastValueAsEnum<int, TEnum>(value);
        }

        public static TEnum ReadByteAsEnum<TEnum>(this BinaryReader reader)
           where TEnum : struct
        {
            var value = reader.ReadByte();

            return EnumUtility.CastValueAsEnum<byte, TEnum>(value);
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        public static RectangleF ReadRectangleF(this BinaryReader reader)
        {
            return new RectangleF(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        public static MapLine2D ReadLine2D(this BinaryReader reader)
        {
            return new MapLine2D(
                reader.ReadVector2(),
                reader.ReadVector2());
        }

        public static IndexedTriangle ReadIndexedTri(this BinaryReader reader)
        {
            return new IndexedTriangle(
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16());
        }

        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            return new Quaternion(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        public static ColorRgbaF ReadColorRgbaF(this BinaryReader reader)
        {
            return new ColorRgbaF(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        public static ColorRgbF ReadColorRgbF(this BinaryReader reader)
        {
            return new ColorRgbF(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        public static ColorRgb ReadColorRgb(this BinaryReader reader)
        {
            return new ColorRgb(
                reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadByte());
        }

        public static ColorRgba ReadColorRgba(this BinaryReader reader)
        {
            return new ColorRgba(
                reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadByte());
        }

        public static uint Align(this BinaryReader reader, uint aligment)
        {
            var pos = reader.BaseStream.Position;
            var calign = ((uint) pos % aligment);
            if (calign == 0)
                return 0;

            var missing = aligment - calign;
            reader.BaseStream.Seek(missing, SeekOrigin.Current);
            return missing;
        }

        public static string ReadStringAtOffset(this BinaryReader reader)
        {
            var stringOffset = reader.ReadUInt32();
            var oldOffset = reader.BaseStream.Position;

            //jump to the location and read the data
            reader.BaseStream.Seek(stringOffset, SeekOrigin.Begin);
            var str = reader.ReadNullTerminatedString();
            reader.BaseStream.Seek(oldOffset, SeekOrigin.Begin);
            return str;
        }

        public static List<T> ReadListAtOffset<T>(this BinaryReader reader, Func<T> creator, bool ptr = false)
        {
            var capacity = reader.ReadInt32();
            List<T> result = new List<T>(capacity);

            //get the offset
            var listOffset = reader.ReadUInt32();
            var oldOffset = reader.BaseStream.Position;

            //jump to the location and read the data
            reader.BaseStream.Seek(listOffset, SeekOrigin.Begin);

            for (var i = 0; i < capacity; i++)
            {
                T item = default(T);
                if (!ptr)
                {
                    item = creator();
                }
                else
                {
                    //read the adress of the object first
                    var itemOffset = reader.ReadUInt32();

                    //if offset is 0 this item must be null
                    if (!itemOffset.Equals(0))
                    {
                        var oldOffset2 = reader.BaseStream.Position;

                        //jump to the location and read the data
                        reader.BaseStream.Seek(itemOffset, SeekOrigin.Begin);
                        item = creator();

                        reader.BaseStream.Seek(oldOffset2, SeekOrigin.Begin);
                    }
                }

                result.Add(item);
            }

            //jump back to where we came from
            reader.BaseStream.Seek(oldOffset, SeekOrigin.Begin);
            return result;
        }

        public static T[] ReadFixedSizeArrayAtOffset<T>(this BinaryReader reader, Func<T> creator, uint size) where T : struct
        {
            var arr = new T[size];

            //get the offset
            var listOffset = reader.ReadUInt32();
            var oldOffset = reader.BaseStream.Position;

            //jump to the location and read the data
            reader.BaseStream.Seek(listOffset, SeekOrigin.Begin);

            for (var i = 0; i < size; i++)
            {
                arr[i] = creator();
            }

            //jump back to where we came from
            reader.BaseStream.Seek(oldOffset, SeekOrigin.Begin);

            return arr;
        }

        public static string ReadFourCc(this BinaryReader reader, bool bigEndian = false)
        {
            var a = reader.ReadChar();
            var b = reader.ReadChar();
            var c = reader.ReadChar();
            var d = reader.ReadChar();

            return bigEndian
                ? new string(new[] { d, c, b, a })
                : new string(new[] { a, b, c, d });
        }

        public static TEnum ReadByteAsEnumFlags<TEnum>(this BinaryReader reader)
           where TEnum : struct
        {
            var value = reader.ReadByte();
            var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), value);

            return enumValue;
        }

        public static TEnum ReadUInt16AsEnumFlags<TEnum>(this BinaryReader reader)
          where TEnum : struct
        {
            var value = reader.ReadUInt16();
            VerifyEnumFlags<TEnum>(value, 16);

            var enumValue = (TEnum) Enum.ToObject(typeof(TEnum), value);
            return enumValue;
        }

        public static TEnum ReadUInt32AsEnumFlags<TEnum>(this BinaryReader reader)
         where TEnum : struct
        {
            var value = reader.ReadUInt32();
            VerifyEnumFlags<TEnum>(value, 32);

            var enumValue = (TEnum) Enum.ToObject(typeof(TEnum), value);
            return enumValue;
        }

        private static void VerifyEnumFlags<TEnum>(uint value, int sizeOfTValue)
            where TEnum : struct
        {
            if (value == 0 && !EnumUtility.IsValueDefined((TEnum) Enum.ToObject(typeof(TEnum), value)))
            {
                throw new InvalidDataException($"Undefined value for flags enum {typeof(TEnum).Name}: 0");
            }

            for (var i = 1; i < sizeOfTValue; i++)
            {
                var maskedValue = value & (1 << i);
                if (maskedValue == 0)
                {
                    continue;
                }
                var enumBitValue = (TEnum) Enum.ToObject(typeof(TEnum), maskedValue);
                if (!EnumUtility.IsValueDefined(enumBitValue))
                {
                    throw new InvalidDataException($"Undefined value for flags enum {typeof(TEnum).Name}: {enumBitValue}");
                }
            }
        }

        public static Matrix2x2 ReadMatrix2x2(this BinaryReader reader)
        {
            return new Matrix2x2()
            {
                M11 = reader.ReadSingle(),
                M12 = reader.ReadSingle(),
                M21 = reader.ReadSingle(),
                M22 = reader.ReadSingle()
            };
        }
    }
}
