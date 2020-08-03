using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats
{
    public static class BinaryReaderExtensions
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

        public static string ReadNullTerminatedAsciiString(this BinaryReader reader)
        {
            var bytes = new List<byte>();
            byte b;
            while ((b = reader.ReadByte()) != '\0')
            {
                bytes.Add(b);
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        public static string ReadBytePrefixedAsciiString(this BinaryReader reader)
        {
            var length = reader.ReadByte();
            return BinaryUtility.AnsiEncoding.GetString(reader.ReadBytes(length));
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

        public static string ReadBytePrefixedUnicodeString(this BinaryReader reader)
        {
            var length = reader.ReadByte();
            return Encoding.Unicode.GetString(reader.ReadBytes(length * 2));
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

        public static uint[,] ReadUIntArray2D(this BinaryReader reader, uint width, uint height, uint bitSize)
        {
            var result = new uint[width, height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    uint value;
                    switch (bitSize)
                    {
                        case 16:
                            value = reader.ReadUInt16();
                            break;

                        case 32:
                            value = reader.ReadUInt32();
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(bitSize));
                    }

                    result[x, y] = value;
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

        public static TEnum ReadUInt24AsEnum<TEnum>(this BinaryReader reader)
           where TEnum : struct
        {
            var value = reader.ReadUInt24();

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

        public static Rectangle ReadRectangle(this BinaryReader reader)
        {
            return new Rectangle(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        public static Point2D ReadPoint2D(this BinaryReader reader)
        {
            return new Point2D(
                reader.ReadInt32(),
                reader.ReadInt32());
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

        public static Matrix4x3 ReadMatrix4x3(this BinaryReader reader)
        {
            return new Matrix4x3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        public static Matrix4x3 ReadMatrix4x3Transposed(this BinaryReader reader)
        {
            var m11 = reader.ReadSingle();
            var m21 = reader.ReadSingle();
            var m31 = reader.ReadSingle();
            var m41 = reader.ReadSingle();

            var m12 = reader.ReadSingle();
            var m22 = reader.ReadSingle();
            var m32 = reader.ReadSingle();
            var m42 = reader.ReadSingle();

            var m13 = reader.ReadSingle();
            var m23 = reader.ReadSingle();
            var m33 = reader.ReadSingle();
            var m43 = reader.ReadSingle();

            return new Matrix4x3(
                m11, m12, m13,
                m21, m22, m23,
                m31, m32, m33,
                m41, m42, m43);
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

        public static ColorRgb ReadColorRgb(this BinaryReader reader, bool extraBytePadding = false)
        {
            var result = new ColorRgb(
                reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadByte());

            if (extraBytePadding)
            {
                reader.ReadByte();
            }

            return result;
        }

        public static ColorRgba ReadColorRgba(this BinaryReader reader, ColorRgbaPixelOrder pixelOrder = ColorRgbaPixelOrder.Rgba)
        {
            byte r, g, b, a;

            switch (pixelOrder)
            {
                case ColorRgbaPixelOrder.Rgba:
                    r = reader.ReadByte();
                    g = reader.ReadByte();
                    b = reader.ReadByte();
                    a = reader.ReadByte();
                    break;

                case ColorRgbaPixelOrder.Bgra:
                    b = reader.ReadByte();
                    g = reader.ReadByte();
                    r = reader.ReadByte();
                    a = reader.ReadByte();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(pixelOrder));
            }

            return new ColorRgba(r, g, b, a);
        }

        public static RandomVariable ReadRandomVariable(this BinaryReader reader)
        {
            var distributionType = reader.ReadUInt32AsEnum<DistributionType>();
            var low = reader.ReadSingle();
            var high = reader.ReadSingle();

            return new RandomVariable(
                low,
                high,
                distributionType);
        }

        public static uint Align(this BinaryReader reader, uint aligment)
        {
            var pos = reader.BaseStream.Position;
            var calign = ((uint) pos % aligment);
            if (calign == 0)
            {
                return 0;
            }

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

        public static string ReadUInt32PrefixedAsciiStringAtOffset(this BinaryReader reader)
        {
            var length = reader.ReadUInt32();

            var stringOffset = reader.ReadUInt32();
            var oldOffset = reader.BaseStream.Position;

            reader.BaseStream.Seek(stringOffset, SeekOrigin.Begin);
            var str = Encoding.ASCII.GetString(reader.ReadBytes((int) length));
            reader.BaseStream.Seek(oldOffset, SeekOrigin.Begin);

            return str;
        }

        // TODO: remove this in favour of ReadArrayAtOffset
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

        public static T[] ReadArrayAtOffset<T>(this BinaryReader reader, Func<T> creator, bool ptr = false)
        {
            var capacity = reader.ReadInt32();
            var result = new T[capacity];

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

                result[i] = item;
            }

            //jump back to where we came from
            reader.BaseStream.Seek(oldOffset, SeekOrigin.Begin);
            return result;
        }

        public static void ReadAtOffset(this BinaryReader reader, Action callback)
        {
            //get the offset
            var listOffset = reader.ReadUInt32();
            var oldOffset = reader.BaseStream.Position;
            
            //jump to the location and read the data
            reader.BaseStream.Seek(listOffset, SeekOrigin.Begin);

            callback();

            //jump back to where we came from
            reader.BaseStream.Seek(oldOffset, SeekOrigin.Begin);
        }

        public static T ReadAtOffset<T>(this BinaryReader reader, Func<T> callback)
        {
            //get the offset
            var listOffset = reader.ReadUInt32();

            if (listOffset == 0)
            {
                return default(T);
            }

            var oldOffset = reader.BaseStream.Position;

            //jump to the location and read the data
            reader.BaseStream.Seek(listOffset, SeekOrigin.Begin);

            var result = callback();

            //jump back to where we came from
            reader.BaseStream.Seek(oldOffset, SeekOrigin.Begin);

            return result;
        }

        public static List<T> ReadFixedSizeListAtOffset<T>(this BinaryReader reader, Func<T> creator, uint size) where T : class
        {
            List<T> result = new List<T>((int)size);

            //get the offset
            var listOffset = reader.ReadUInt32();
            var oldOffset = reader.BaseStream.Position;

            //jump to the location and read the data
            reader.BaseStream.Seek(listOffset, SeekOrigin.Begin);

            for (var i = 0; i < size; i++)
            {
                result.Add(creator());
            }

            //jump back to where we came from
            reader.BaseStream.Seek(oldOffset, SeekOrigin.Begin);

            return result;
        }

        public static T[] ReadFixedSizeArrayAtOffset<T>(this BinaryReader reader, Func<T> creator, uint size)
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
            var a = (char) reader.ReadByte();
            var b = (char) reader.ReadByte();
            var c = (char) reader.ReadByte();
            var d = (char) reader.ReadByte();

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
            return new Matrix2x2(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        public static Line2D ReadLine2D(this BinaryReader reader)
        {
            return new Line2D(
                reader.ReadVector2(),
                reader.ReadVector2());
        }

        public static TimeSpan ReadTime(this BinaryReader reader)
        {
            return TimeSpan.FromSeconds(reader.ReadSingle());
        }

        public static DateTime ReadDateTime(this BinaryReader reader)
        {
            var year = reader.ReadUInt16();
            var month = reader.ReadUInt16();
            var day = reader.ReadUInt16();
            _ = reader.ReadUInt16AsEnum<DayOfWeek>();
            var hour = reader.ReadUInt16();
            var minutes = reader.ReadUInt16();
            var seconds = reader.ReadUInt16();
            var milliseconds = reader.ReadUInt16();

            return new DateTime(
                year, month, day,
                hour, minutes, seconds, milliseconds);
        }

        public static Percentage ReadPercentage(this BinaryReader reader)
        {
            return new Percentage(reader.ReadSingle());
        }

        public static FloatRange ReadFloatRange(this BinaryReader reader)
        {
            return new FloatRange(reader.ReadSingle(), reader.ReadSingle());
        }

        public static IntRange ReadIntRange(this BinaryReader reader)
        {
            return new IntRange(reader.ReadInt32(), reader.ReadInt32());
        }

        public static T? ReadOptionalValueAtOffset<T>(this BinaryReader reader, Func<T> readCallback)
            where T : struct
        {
            var offset = reader.ReadUInt32();
            if (offset > 0)
            {
                var current = reader.BaseStream.Position;
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var value = readCallback();
                reader.BaseStream.Seek(current, SeekOrigin.Begin);
                return value;
            }
            else
            {
                return null;
            }

        }

        public static T ReadOptionalClassTypedValueAtOffset<T>(this BinaryReader reader, Func<T> readCallback)
            where T : class
        {
            var offset = reader.ReadUInt32();
            if (offset > 0)
            {
                var current = reader.BaseStream.Position;
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var value = readCallback();
                reader.BaseStream.Seek(current, SeekOrigin.Begin);
                return value;
            }
            else
            {
                return null;
            }
        }

        public static byte ReadVersion(this BinaryReader reader) => reader.ReadByte();
    }

    public enum ColorRgbaPixelOrder
    {
        Rgba,
        Bgra
    }
}
