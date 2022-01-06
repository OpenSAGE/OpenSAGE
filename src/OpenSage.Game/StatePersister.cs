using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using OpenSage.Content;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage
{
    public abstract class StatePersister : DisposableBase
    {
        protected readonly Stack<Segment> Segments;

        public readonly StatePersistMode Mode;

        public readonly Game Game;
        public readonly SageGame SageGame;
        public readonly AssetStore AssetStore;

        protected StatePersister(Game game, StatePersistMode mode)
        {
            Segments = new Stack<Segment>();

            Mode = mode;

            Game = game;
            SageGame = game.SageGame;
            AssetStore = game.AssetStore;
        }

        public byte PersistVersion(byte maximumVersion)
        {
            var actualVersion = maximumVersion;
            PersistByte("Version", ref actualVersion);

            if (actualVersion == 0 || actualVersion > maximumVersion)
            {
                throw new InvalidStateException();
            }

            return actualVersion;
        }

        public virtual void PersistFieldName(string name) { }

        public void PersistByte(string name, ref byte value)
        {
            PersistFieldName(name);

            PersistByteValue(ref value);
        }

        public abstract void PersistByteValue(ref byte value);

        public void PersistInt16(string name, ref short value)
        {
            PersistFieldName(name);

            PersistInt16Value(ref value);
        }

        public abstract void PersistInt16Value(ref short value);

        public void PersistUInt16(string name, ref ushort value)
        {
            PersistFieldName(name);

            PersistUInt16Value(ref value);
        }

        public abstract void PersistUInt16Value(ref ushort value);

        public void PersistInt32(string name, ref int value)
        {
            PersistFieldName(name);

            PersistInt32Value(ref value);
        }

        public abstract void PersistInt32Value(ref int value);

        public void PersistUInt32(string name, ref uint value)
        {
            PersistFieldName(name);

            PersistUInt32Value(ref value);
        }

        public abstract void PersistUInt32Value(ref uint value);

        public void PersistBoolean(string name, ref bool value)
        {
            PersistFieldName(name);

            PersistBooleanValue(ref value);
        }

        public abstract void PersistBooleanValue(ref bool value);

        public void PersistAsciiString(string name, ref string value)
        {
            PersistFieldName(name);

            PersistAsciiStringValue(ref value);
        }

        public abstract void PersistAsciiStringValue(ref string value);

        public void PersistUnicodeString(string name, ref string value)
        {
            PersistFieldName(name);

            PersistUnicodeStringValue(ref value);
        }

        public abstract void PersistUnicodeStringValue(ref string value);

        public void PersistSingle(string name, ref float value)
        {
            PersistFieldName(name);

            PersistSingleValue(ref value);
        }

        public abstract void PersistSingleValue(ref float value);

        public void PersistEnum<TEnum>(string name, ref TEnum value)
            where TEnum : struct
        {
            PersistFieldName(name);

            PersistEnumValue(ref value);
        }

        public abstract void PersistEnumValue<TEnum>(ref TEnum value)
            where TEnum : struct;

        public void PersistEnumByte<TEnum>(string name, ref TEnum value)
            where TEnum : struct
        {
            PersistFieldName(name);

            PersistEnumByteValue(ref value);
        }

        public abstract void PersistEnumByteValue<TEnum>(ref TEnum value)
            where TEnum : struct;

        public void PersistEnumFlags<TEnum>(string name, ref TEnum value)
            where TEnum : struct
        {
            PersistFieldName(name);

            PersistEnumFlagsValue(ref value);
        }

        public abstract void PersistEnumFlagsValue<TEnum>(ref TEnum value)
            where TEnum : struct;

        public void PersistEnumByteFlags<TEnum>(string name, ref TEnum value)
            where TEnum : struct
        {
            PersistFieldName(name);

            PersistEnumByteFlagsValue(ref value);
        }

        public abstract void PersistEnumByteFlagsValue<TEnum>(ref TEnum value)
            where TEnum : struct;

        public void PersistObject<T>(string name, T value)
            where T : class, IPersistableObject
        {
            PersistFieldName(name);

            PersistObjectValue(value);
        }

        public void PersistObject<T>(string name, ref T value)
            where T : struct, IPersistableObject
        {
            PersistFieldName(name);

            PersistObjectValue(ref value);
        }

        public void PersistObjectValue<T>(T value)
            where T : class, IPersistableObject
        {
            BeginObject();

            OnPersistObjectValue(value);

            value.Persist(this);

            EndObject();
        }

        public void PersistObjectValue<T>(ref T value)
            where T : struct, IPersistableObject
        {
            BeginObject();

            OnPersistObjectValue(ref value);

            value.Persist(this);

            EndObject();
        }

        protected virtual void OnPersistObjectValue<T>(T value)
            where T : class, IPersistableObject
        {
        }

        protected virtual void OnPersistObjectValue<T>(ref T value)
            where T : struct, IPersistableObject
        {
        }

        public abstract void PersistSpan(Span<byte> span);

        public abstract uint BeginSegment(string segmentName);

        public abstract void EndSegment();

        protected record struct Segment(long Start, long End, string Name);

        public void BeginObject(string name)
        {
            PersistFieldName(name);

            BeginObject();
        }

        public virtual void BeginObject() { }

        public virtual void EndObject() { }

        public void BeginArray(string name)
        {
            PersistFieldName(name);

            BeginArray();
        }

        public virtual void BeginArray() { }

        public virtual void EndArray() { }

        public abstract void SkipUnknownBytes(int numBytes);
    }

    public enum StatePersistMode
    {
        Read,
        Write,
    }

    public sealed class StateReader : StatePersister
    {
        private readonly BinaryReader _binaryReader;

        internal StateReader(Stream stream, Game game)
            : base(game, StatePersistMode.Read)
        {
            _binaryReader = AddDisposable(new BinaryReader(stream, Encoding.Unicode, true));
        }

        public override void PersistByteValue(ref byte value) => value = _binaryReader.ReadByte();

        public override void PersistInt16Value(ref short value) => value = _binaryReader.ReadInt16();

        public override void PersistUInt16Value(ref ushort value) => value = _binaryReader.ReadUInt16();

        public override void PersistInt32Value(ref int value) => value = _binaryReader.ReadInt32();

        public override void PersistUInt32Value(ref uint value) => value = _binaryReader.ReadUInt32();

        public override void PersistBooleanValue(ref bool value) => value = _binaryReader.ReadBoolean();

        public override void PersistAsciiStringValue(ref string value) => value = _binaryReader.ReadBytePrefixedAsciiString();

        public override void PersistUnicodeStringValue(ref string value) => value = _binaryReader.ReadBytePrefixedUnicodeString();

        public override void PersistSingleValue(ref float value) => value = _binaryReader.ReadSingle();

        public override void PersistEnumValue<TEnum>(ref TEnum value) => value = _binaryReader.ReadUInt32AsEnum<TEnum>();

        public override void PersistEnumByteValue<TEnum>(ref TEnum value) => value = _binaryReader.ReadByteAsEnum<TEnum>();

        public override void PersistEnumFlagsValue<TEnum>(ref TEnum value) => value = _binaryReader.ReadUInt32AsEnumFlags<TEnum>();

        public override void PersistEnumByteFlagsValue<TEnum>(ref TEnum value) => value = _binaryReader.ReadByteAsEnumFlags<TEnum>();

        public override void PersistSpan(Span<byte> span) => _binaryReader.BaseStream.Read(span);

        public override uint BeginSegment(string segmentName)
        {
            if (SageGame >= SageGame.Bfme)
            {
                var blokHeader = _binaryReader.ReadFourCc(bigEndian: true);
                if (blokHeader != "BLOK")
                {
                    throw new InvalidStateException();
                }

                var segmentEnd = _binaryReader.ReadUInt32();

                var currentPosition = _binaryReader.BaseStream.Position;

                var segmentLength = (uint)(segmentEnd - currentPosition);

                Segments.Push(new Segment(currentPosition, segmentEnd, segmentName));

                return segmentLength;
            }
            else
            {
                var segmentLength = _binaryReader.ReadUInt32();

                var currentPosition = _binaryReader.BaseStream.Position;

                Segments.Push(new Segment(currentPosition, currentPosition + segmentLength, segmentName));

                return segmentLength;
            }
        }

        public override void EndSegment()
        {
            var segment = Segments.Pop();

            if (_binaryReader.BaseStream.Position != segment.End)
            {
                throw new InvalidStateException($"Stream position expected to be at 0x{segment.End:X8} but was at 0x{_binaryReader.BaseStream.Position:X8} while reading {segment.Name}");
            }
        }

        public override void SkipUnknownBytes(int numBytes)
        {
            for (var i = 0; i < numBytes; i++)
            {
                var value = _binaryReader.ReadByte();

                if (value != 0)
                {
                    throw new InvalidStateException($"Expected byte (index {i}) to be 0 but it was {value}");
                }
            }
        }
    }

    public sealed class StateWriter : StatePersister
    {
        private readonly BinaryWriter _binaryWriter;

        internal StateWriter(Stream stream, Game game)
            : base(game, StatePersistMode.Read)
        {
            _binaryWriter = AddDisposable(new BinaryWriter(stream, Encoding.Unicode, true));
        }

        public override void PersistByteValue(ref byte value) => _binaryWriter.Write(value);

        public override void PersistInt16Value(ref short value) => _binaryWriter.Write(value);

        public override void PersistUInt16Value(ref ushort value) => _binaryWriter.Write(value);

        public override void PersistInt32Value(ref int value) => _binaryWriter.Write(value);

        public override void PersistUInt32Value(ref uint value) => _binaryWriter.Write(value);

        public override void PersistBooleanValue(ref bool value) => _binaryWriter.Write(value);

        public override void PersistAsciiStringValue(ref string value) => _binaryWriter.WriteBytePrefixedAsciiString(value);

        public override void PersistUnicodeStringValue(ref string value) => _binaryWriter.WriteBytePrefixedUnicodeString(value);

        public override void PersistSingleValue(ref float value) => _binaryWriter.Write(value);

        public override void PersistEnumValue<TEnum>(ref TEnum value) => _binaryWriter.WriteEnumAsUInt32(value);

        public override void PersistEnumByteValue<TEnum>(ref TEnum value) => _binaryWriter.WriteEnumAsByte(value);

        public override void PersistEnumFlagsValue<TEnum>(ref TEnum value) => _binaryWriter.WriteEnumAsUInt32(value);

        public override void PersistEnumByteFlagsValue<TEnum>(ref TEnum value) => _binaryWriter.WriteEnumAsByte(value);

        public override void PersistSpan(Span<byte> span) => _binaryWriter.BaseStream.Write(span);

        public override uint BeginSegment(string segmentName)
        {
            if (SageGame >= SageGame.Bfme)
            {
                throw new NotImplementedException();
            }
            else
            {
                // Write placeholder for segment length - we'll patch this later.
                _binaryWriter.Write(0u);

                var currentPosition = _binaryWriter.BaseStream.Position;

                Segments.Push(new Segment(currentPosition, currentPosition, segmentName));

                return 0u;
            }
        }

        public override void EndSegment()
        {
            var segment = Segments.Pop();

            var currentPosition = _binaryWriter.BaseStream.Position;
            var segmentLength = currentPosition - segment.Start;

            _binaryWriter.BaseStream.Position = segment.Start - 4;
            _binaryWriter.Write((uint)segmentLength);
            _binaryWriter.BaseStream.Position = currentPosition;
        }

        public override void SkipUnknownBytes(int numBytes)
        {
            for (var i = 0; i < numBytes; i++)
            {
                _binaryWriter.Write((byte)0);
            }
        }
    }

    public sealed class InvalidStateException : Exception
    {
        public InvalidStateException()
            : base()
        {

        }

        public InvalidStateException(string message)
            : base(message)
        {

        }
    }

    public interface IPersistableObject
    {
        void Persist(StatePersister persister);
    }

    public struct ObjectNameAndId : IPersistableObject
    {
        public string Name;
        public uint ObjectId;

        public void Persist(StatePersister persister)
        {
            persister.PersistAsciiString("Name", ref Name);
            persister.PersistObjectID("ObjectId", ref ObjectId);
        }
    }

    public static class StatePersisterExtensions
    {
        public static void PersistObjectID(this StatePersister persister, string name, ref uint value)
        {
            persister.PersistFieldName(name);

            persister.PersistObjectIDValue(ref value);
        }

        public static void PersistObjectIDValue(this StatePersister persister, ref uint value) => persister.PersistUInt32Value(ref value);

        public static void PersistFrame(this StatePersister persister, string name, ref uint value)
        {
            persister.PersistFieldName(name);

            persister.PersistFrameValue(ref value);
        }

        public static void PersistFrameValue(this StatePersister persister, ref uint value) => persister.PersistUInt32Value(ref value);

        public static void PersistMatrix4x3(this StatePersister persister, string name, ref Matrix4x3 value, bool readVersion = true)
        {
            persister.BeginObject(name);

            persister.PersistMatrix4x3Impl(ref value, readVersion);

            persister.EndObject();
        }

        public static void PersistMatrix4x3Value(this StatePersister persister, ref Matrix4x3 value, bool readVersion = true)
        {
            persister.BeginObject();

            persister.PersistMatrix4x3Impl(ref value, readVersion);

            persister.EndObject();
        }

        private static void PersistMatrix4x3Impl(this StatePersister persister, ref Matrix4x3 value, bool readVersion)
        {
            if (readVersion)
            {
                persister.PersistVersion(1);
            }

            var m11 = value.M11;
            persister.PersistSingle("M11", ref m11);

            var m21 = value.M21;
            persister.PersistSingle("M21", ref m21);

            var m31 = value.M31;
            persister.PersistSingle("M31", ref m31);

            var m41 = value.M41;
            persister.PersistSingle("M41", ref m41);

            var m12 = value.M12;
            persister.PersistSingle("M12", ref m12);

            var m22 = value.M22;
            persister.PersistSingle("M22", ref m22);

            var m32 = value.M32;
            persister.PersistSingle("M32", ref m32);

            var m42 = value.M42;
            persister.PersistSingle("M42", ref m42);

            var m13 = value.M13;
            persister.PersistSingle("M13", ref m13);

            var m23 = value.M23;
            persister.PersistSingle("M23", ref m23);

            var m33 = value.M33;
            persister.PersistSingle("M33", ref m33);

            var m43 = value.M43;
            persister.PersistSingle("M43", ref m43);

            if (persister.Mode == StatePersistMode.Read)
            {
                value = new Matrix4x3(
                    m11, m12, m13,
                    m21, m22, m23,
                    m31, m32, m33,
                    m41, m42, m43);
            }
        }

        public static void PersistBitArray<TEnum>(this StatePersister persister, string name, ref BitArray<TEnum> result)
            where TEnum : Enum
        {
            persister.BeginObject(name);

            persister.PersistVersion(1);

            if (persister.Mode == StatePersistMode.Read)
            {
                result.SetAll(false);
            }

            var count = (uint)result.NumBitsSet;
            persister.PersistUInt32("Count", ref count);

            persister.BeginArray("Items");

            if (persister.Mode == StatePersistMode.Read)
            {
                var stringToValueMap = Data.Ini.IniParser.GetEnumMap<TEnum>();

                for (var i = 0; i < count; i++)
                {
                    string stringValue = default;
                    persister.PersistAsciiStringValue(ref stringValue);

                    var enumValue = (TEnum)stringToValueMap[stringValue];

                    result.Set(enumValue, true);
                }
            }
            else
            {
                var valueToStringMap = Data.Ini.IniParser.GetEnumMapReverse<TEnum>();

                foreach (var setBit in result.GetSetBits())
                {
                    var stringValue = valueToStringMap[setBit];

                    persister.PersistAsciiStringValue(ref stringValue);
                }
            }

            persister.EndArray();

            persister.EndObject();
        }

        public static void PersistVector3(this StatePersister persister, string name, ref Vector3 value)
        {
            persister.BeginObject(name);

            persister.PersistSingle("X", ref value.X);
            persister.PersistSingle("Y", ref value.Y);
            persister.PersistSingle("Z", ref value.Z);

            persister.EndObject();
        }

        public static void PersistVector3Value(this StatePersister persister, ref Vector3 value)
        {
            persister.BeginObject();

            persister.PersistSingle("X", ref value.X);
            persister.PersistSingle("Y", ref value.Y);
            persister.PersistSingle("Z", ref value.Z);

            persister.EndObject();
        }

        public static void PersistPoint2D(this StatePersister persister, string name, ref Point2D value)
        {
            persister.BeginObject(name);

            var x = value.X;
            persister.PersistInt32("X", ref x);

            var y = value.Y;
            persister.PersistInt32("Y", ref y);

            if (persister.Mode == StatePersistMode.Read)
            {
                value = new Point2D(x, y);
            }

            persister.EndObject();
        }

        public static void PersistPoint3D(this StatePersister persister, string name, ref Point3D value)
        {
            persister.BeginObject(name);

            persister.PersistPoint3DImpl(ref value);

            persister.EndObject();
        }

        public static void PersistPoint3DValue(this StatePersister persister, ref Point3D value)
        {
            persister.BeginObject();

            persister.PersistPoint3DImpl(ref value);

            persister.EndObject();
        }

        private static void PersistPoint3DImpl(this StatePersister persister, ref Point3D value)
        {
            var x = value.X;
            persister.PersistInt32("X", ref x);

            var y = value.Y;
            persister.PersistInt32("Y", ref y);

            var z = value.Z;
            persister.PersistInt32("Z", ref z);

            if (persister.Mode == StatePersistMode.Read)
            {
                value = new Point3D(x, y, z);
            }
        }

        public static void PersistColorRgbF(this StatePersister persister, string name, ref ColorRgbF value)
        {
            persister.BeginObject(name);

            var r = value.R;
            persister.PersistSingle("R", ref r);

            var g = value.G;
            persister.PersistSingle("G", ref g);

            var b = value.B;
            persister.PersistSingle("B", ref b);

            persister.EndObject();

            if (persister.Mode == StatePersistMode.Read)
            {
                value = new ColorRgbF(r, g, b);
            }
        }

        public static void PersistColorRgba(this StatePersister persister, string name, ref ColorRgba value)
        {
            persister.BeginObject(name);

            var r = value.R;
            persister.PersistByte("R", ref r);

            var g = value.G;
            persister.PersistByte("G", ref g);

            var b = value.B;
            persister.PersistByte("B", ref b);

            var a = value.A;
            persister.PersistByte("A", ref a);

            persister.EndObject();

            if (persister.Mode == StatePersistMode.Read)
            {
                value = new ColorRgba(r, g, b, a);
            }
        }

        public static void PersistColorRgbaInt(this StatePersister persister, string name, ref ColorRgba value)
        {
            persister.BeginObject(name);

            var r = (int)value.R;
            persister.PersistInt32("R", ref r);

            var g = (int)value.G;
            persister.PersistInt32("G", ref g);

            var b = (int)value.B;
            persister.PersistInt32("B", ref b);

            var a = (int)value.A;
            persister.PersistInt32("A", ref a);

            persister.EndObject();

            if (persister.Mode == StatePersistMode.Read)
            {
                if (r > 255 || g > 255 || b > 255 || a > 255)
                {
                    throw new InvalidOperationException();
                }

                value = new ColorRgba((byte)r, (byte)g, (byte)b, (byte)a);
            }
        }

        public static void PersistDateTime(this StatePersister persister, string name, ref DateTime value)
        {
            persister.BeginObject(name);

            var year = (ushort)value.Year;
            persister.PersistUInt16("Year", ref year);

            var month = (ushort)value.Month;
            persister.PersistUInt16("Month", ref month);

            var day = (ushort)value.Day;
            persister.PersistUInt16("Day", ref day);

            var dayOfWeek = (ushort)value.DayOfWeek;
            persister.PersistUInt16("DayOfWeek", ref dayOfWeek);

            var hour = (ushort)value.Hour;
            persister.PersistUInt16("Hour", ref hour);

            var minute = (ushort)value.Minute;
            persister.PersistUInt16("Minute", ref minute);

            var second = (ushort)value.Second;
            persister.PersistUInt16("Second", ref second);

            var millisecond = (ushort)value.Millisecond;
            persister.PersistUInt16("Millisecond", ref millisecond);

            persister.EndObject();

            if (persister.Mode == StatePersistMode.Read)
            {
                value = new DateTime(year, month, day, hour, minute, second, millisecond);
            }
        }

        public static void PersistRandomVariable(this StatePersister persister, string name, ref RandomVariable value)
        {
            persister.BeginObject(name);

            var distributionType = value.DistributionType;
            persister.PersistEnum("DistributionType", ref distributionType);

            var low = value.Low;
            persister.PersistSingle("Low", ref low);

            var high = value.High;
            persister.PersistSingle("High", ref high);

            persister.EndObject();

            if (persister.Mode == StatePersistMode.Read)
            {
                value = new RandomVariable(low, high, distributionType);
            }
        }

        public static void PersistArray<T>(this StatePersister persister, string name, T[] value, PersistListItemCallback<T> callback)
        {
            persister.BeginArray(name);

            for (var i = 0; i < value.Length; i++)
            {
                callback(persister, ref value[i]);
            }

            persister.EndArray();
        }

        public static void PersistArrayWithUInt16Length<T>(this StatePersister persister, string name, T[] value, PersistListItemCallback<T> callback)
        {
            persister.BeginObject(name);

            var length = (ushort)value.Length;
            persister.PersistUInt16("Length", ref length);

            if (length != value.Length)
            {
                throw new InvalidStateException();
            }

            PersistArray(persister, "Items", value, callback);

            persister.EndObject();
        }

        public static void PersistArrayWithUInt32Length<T>(this StatePersister persister, string name, T[] value, PersistListItemCallback<T> callback)
        {
            persister.BeginObject(name);

            var length = (uint)value.Length;
            persister.PersistUInt32("Length", ref length);

            if (length != value.Length)
            {
                throw new InvalidStateException();
            }

            PersistArray(persister, "Items", value, callback);

            persister.EndObject();
        }

        public static void PersistHashSet<T>(this StatePersister persister, string name, HashSet<T> value, PersistListItemCallback<T> callback)
        {
            persister.BeginObject(name);

            persister.PersistHashSetValueImpl(value, callback);

            persister.EndObject();
        }

        public static void PersistHashSetValue<T>(this StatePersister persister, HashSet<T> value, PersistListItemCallback<T> callback)
        {
            persister.BeginObject();

            persister.PersistHashSetValueImpl(value, callback);

            persister.EndObject();
        }

        private static void PersistHashSetValueImpl<T>(this StatePersister persister, HashSet<T> value, PersistListItemCallback<T> callback)
        {
            var count = (ushort)value.Count;
            persister.PersistUInt16("Count", ref count);

            persister.BeginArray("Items");

            if (persister.Mode == StatePersistMode.Read)
            {
                value.Clear();

                for (var i = 0; i < count; i++)
                {
                    var item = default(T);
                    callback(persister, ref item);
                    value.Add(item);
                }
            }
            else
            {
                foreach (var item in value)
                {
                    var itemCopy = item;
                    callback(persister, ref itemCopy);
                }
            }

            persister.EndArray();
        }

        public delegate void PersistDictionaryItemCallback<TKey, TValue>(StatePersister persister, ref TKey key, ref TValue value);

        public static void PersistDictionary<TKey, TValue>(this StatePersister persister, string name, Dictionary<TKey, TValue> value, PersistDictionaryItemCallback<TKey, TValue> callback)
        {
            persister.BeginObject(name);

            var count = (ushort)value.Count;
            persister.PersistUInt16("Count", ref count);

            persister.PersistDictionaryImpl(value, count, callback);

            persister.EndObject();
        }

        public static void PersistDictionaryWithUInt32Count<TKey, TValue>(this StatePersister persister, string name, Dictionary<TKey, TValue> value, PersistDictionaryItemCallback<TKey, TValue> callback)
        {
            persister.BeginObject(name);

            var count = (uint)value.Count;
            persister.PersistUInt32("Count", ref count);

            persister.PersistDictionaryImpl(value, count, callback);

            persister.EndObject();
        }

        public static void PersistDictionaryValue<TKey, TValue>(this StatePersister persister, Dictionary<TKey, TValue> value, PersistDictionaryItemCallback<TKey, TValue> callback)
        {
            persister.BeginObject();

            var count = (ushort)value.Count;
            persister.PersistUInt16("Count", ref count);

            persister.PersistDictionaryImpl(value, count, callback);

            persister.EndObject();
        }

        private static void PersistDictionaryImpl<TKey, TValue>(this StatePersister persister, Dictionary<TKey, TValue> value, uint count, PersistDictionaryItemCallback<TKey, TValue> callback)
        {
            persister.BeginArray("Items");

            if (persister.Mode == StatePersistMode.Read)
            {
                value.Clear();

                for (var i = 0; i < count; i++)
                {
                    persister.BeginObject();

                    var itemKey = default(TKey);
                    var itemValue = default(TValue);
                    callback(persister, ref itemKey, ref itemValue);
                    value.Add(itemKey, itemValue);

                    persister.EndObject();
                }
            }
            else
            {
                foreach (var item in value)
                {
                    persister.BeginObject();

                    var itemKey = item.Key;
                    var itemValue = item.Value;
                    callback(persister, ref itemKey, ref itemValue);

                    persister.EndObject();
                }
            }

            persister.EndArray();
        }

        public delegate void PersistListItemCallback<T>(StatePersister persister, ref T item);

        public static void PersistList<T>(this StatePersister persister, string name, List<T> value, PersistListItemCallback<T> callback)
        {
            persister.BeginObject(name);

            var count = (ushort)value.Count;
            persister.PersistUInt16("Count", ref count);

            PersistListImpl(persister, value, count, callback);

            persister.EndObject();
        }

        public static void PersistListWithByteCount<T>(this StatePersister persister, string name, List<T> value, PersistListItemCallback<T> callback)
        {
            persister.BeginObject(name);

            var count = (byte)value.Count;
            persister.PersistByte("Count", ref count);

            PersistListImpl(persister, value, count, callback);

            persister.EndObject();
        }

        public static void PersistListWithByteCountValue<T>(this StatePersister persister, List<T> value, PersistListItemCallback<T> callback)
        {
            persister.BeginObject();

            var count = (byte)value.Count;
            persister.PersistByte("Count", ref count);

            PersistListImpl(persister, value, count, callback);

            persister.EndObject();
        }

        public static void PersistListWithUInt32Count<T>(this StatePersister persister, string name, List<T> value, PersistListItemCallback<T> callback)
        {
            persister.BeginObject(name);

            var count = (uint)value.Count;
            persister.PersistUInt32("Count", ref count);

            PersistListImpl(persister, value, count, callback);

            persister.EndObject();
        }

        private static void PersistListImpl<T>(this StatePersister persister, List<T> value, uint count, PersistListItemCallback<T> callback)
        {
            persister.BeginArray("Items");

            if (persister.Mode == StatePersistMode.Read)
            {
                value.Clear();

                for (var i = 0; i < count; i++)
                {
                    var item = default(T);
                    callback(persister, ref item);
                    value.Add(item);
                }
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    var item = value[i];
                    callback(persister, ref item);
                }
            }

            persister.EndArray();
        }

        public static void PersistObjectNameAndIdList(this StatePersister persister, string name, List<ObjectNameAndId> value)
        {
            persister.BeginObject();

            persister.PersistVersion(1);

            persister.PersistList(name, value, static (StatePersister persister, ref ObjectNameAndId item) =>
            {
                persister.PersistObjectValue(ref item);
            });

            persister.EndObject();
        }
    }
}
