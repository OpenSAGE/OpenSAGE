using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using OpenSage.Content;
using OpenSage.FileFormats;
using OpenSage.Graphics.ParticleSystems;
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

        public abstract byte PersistVersion(byte maximumVersion);

        public abstract void PersistByte(ref byte value);

        public abstract void PersistInt16(ref short value);

        public abstract void PersistUInt16(ref ushort value);

        public abstract void PersistInt32(ref int value);

        public abstract void PersistUInt32(ref uint value);

        public abstract void PersistBoolean(ref bool value);

        public void PersistObjectID(ref uint value) => PersistUInt32(ref value);

        public void PersistFrame(ref uint value) => PersistUInt32(ref value);

        public abstract void PersistAsciiString(ref string value);

        public abstract void PersistUnicodeString(ref string value);

        public abstract void PersistSingle(ref float value);

        public void PersistVector3(ref Vector3 value)
        {
            PersistSingle(ref value.X);
            PersistSingle(ref value.Y);
            PersistSingle(ref value.Z);
        }

        public void PersistPoint2D(ref Point2D value)
        {
            var x = value.X;
            PersistInt32(ref x);

            var y = value.Y;
            PersistInt32(ref y);

            if (Mode == StatePersistMode.Read)
            {
                value = new Point2D(x, y);
            }
        }

        public void PersistPoint3D(ref Point3D value)
        {
            var x = value.X;
            PersistInt32(ref x);

            var y = value.Y;
            PersistInt32(ref y);

            var z = value.Z;
            PersistInt32(ref z);

            if (Mode == StatePersistMode.Read)
            {
                value = new Point3D(x, y, z);
            }
        }

        public abstract void PersistEnum<TEnum>(ref TEnum value)
            where TEnum : struct;

        public abstract void PersistEnumByte<TEnum>(ref TEnum value)
            where TEnum : struct;

        public abstract void PersistEnumFlags<TEnum>(ref TEnum value)
            where TEnum : struct;

        public abstract void PersistEnumByteFlags<TEnum>(ref TEnum value)
            where TEnum : struct;

        public abstract void PersistMatrix4x3(ref Matrix4x3 value, bool readVersion = true);

        public abstract void PersistBitArray<TEnum>(ref BitArray<TEnum> result)
            where TEnum : Enum;

        public void PersistColorRgbF(ref ColorRgbF value)
        {
            var r = value.R;
            PersistSingle(ref r);

            var g = value.G;
            PersistSingle(ref g);

            var b = value.B;
            PersistSingle(ref b);

            if (Mode == StatePersistMode.Read)
            {
                value = new ColorRgbF(r, g, b);
            }
        }

        public void PersistColorRgba(ref ColorRgba value)
        {
            var r = value.R;
            PersistByte(ref r);

            var g = value.G;
            PersistByte(ref g);

            var b = value.B;
            PersistByte(ref b);

            var a = value.A;
            PersistByte(ref a);

            if (Mode == StatePersistMode.Read)
            {
                value = new ColorRgba(r, g, b, a);
            }
        }

        public void PersistColorRgbaInt(ref ColorRgba value)
        {
            var r = (int)value.R;
            PersistInt32(ref r);

            var g = (int)value.G;
            PersistInt32(ref g);

            var b = (int)value.B;
            PersistInt32(ref b);

            var a = (int)value.A;
            PersistInt32(ref a);

            if (Mode == StatePersistMode.Read)
            {
                if (r > 255 || g > 255 || b > 255 || a > 255)
                {
                    throw new InvalidOperationException();
                }

                value = new ColorRgba((byte)r, (byte)g, (byte)b, (byte)a);
            }
        }

        public abstract void PersistDateTime(ref DateTime value);

        public void PersistRandomVariable(ref RandomVariable value)
        {
            var distributionType = value.DistributionType;
            PersistEnum(ref distributionType);

            var low = value.Low;
            PersistSingle(ref low);

            var high = value.High;
            PersistSingle(ref high);

            if (Mode == StatePersistMode.Read)
            {
                value = new RandomVariable(low, high, distributionType);
            }
        }

        public void PersistRandomAlphaKeyframe(ref RandomAlphaKeyframe value)
        {
            var randomVariable = value.Value;
            PersistRandomVariable(ref randomVariable);

            var time = value.Time;
            PersistUInt32(ref time);

            if (Mode == StatePersistMode.Read)
            {
                value = new RandomAlphaKeyframe(randomVariable, time);
            }
        }

        public void PersistRgbColorKeyframe(ref RgbColorKeyframe value)
        {
            var color = value.Color;
            PersistColorRgbF(ref color);

            var time = value.Time;
            PersistUInt32(ref time);

            if (Mode == StatePersistMode.Read)
            {
                value = new RgbColorKeyframe(color, time);
            }
        }

        public delegate void PersistListItemCallback<T>(StatePersister persister, ref T item);

        public abstract void PersistList<T>(List<T> value, PersistListItemCallback<T> callback)
            where T : new();

        public abstract void ReadBytesIntoStream(Stream destination, int numBytes);

        public abstract uint BeginSegment(string segmentName);

        public abstract void EndSegment();

        protected record struct Segment(long Start, long End, string Name);

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

        public override byte PersistVersion(byte maximumVersion)
        {
            var result = _binaryReader.ReadByte();
            if (result == 0 || result > maximumVersion)
            {
                throw new InvalidStateException();
            }
            return result;
        }

        public override void PersistByte(ref byte value) => value = _binaryReader.ReadByte();

        public override void PersistInt16(ref short value) => value = _binaryReader.ReadInt16();

        public override void PersistUInt16(ref ushort value) => value = _binaryReader.ReadUInt16();

        public override void PersistInt32(ref int value) => value = _binaryReader.ReadInt32();

        public override void PersistUInt32(ref uint value) => value = _binaryReader.ReadUInt32();

        public override void PersistBoolean(ref bool value) => value = _binaryReader.ReadBooleanChecked();

        public override void PersistAsciiString(ref string value) => value = _binaryReader.ReadBytePrefixedAsciiString();

        public override void PersistUnicodeString(ref string value) => value = _binaryReader.ReadBytePrefixedUnicodeString();

        public override void PersistSingle(ref float value) => value = _binaryReader.ReadSingle();

        public override void PersistEnum<TEnum>(ref TEnum value) => value = _binaryReader.ReadUInt32AsEnum<TEnum>();

        public override void PersistEnumByte<TEnum>(ref TEnum value) => value = _binaryReader.ReadByteAsEnum<TEnum>();

        public override void PersistEnumFlags<TEnum>(ref TEnum value) => value = _binaryReader.ReadUInt32AsEnumFlags<TEnum>();

        public override void PersistEnumByteFlags<TEnum>(ref TEnum value) => value = _binaryReader.ReadByteAsEnumFlags<TEnum>();

        public override void PersistMatrix4x3(ref Matrix4x3 value, bool readVersion)
        {
            if (readVersion)
            {
                PersistVersion(1);
            }

            value = _binaryReader.ReadMatrix4x3Transposed();
        }

        public override void PersistBitArray<TEnum>(ref BitArray<TEnum> result)
        {
            PersistVersion(1);

            result.SetAll(false);

            var count = _binaryReader.ReadUInt32();

            var stringToValueMap = Data.Ini.IniParser.GetEnumMap<TEnum>();

            for (var i = 0; i < count; i++)
            {
                var stringValue = _binaryReader.ReadBytePrefixedAsciiString();

                var enumValue = (TEnum)stringToValueMap[stringValue];

                result.Set(enumValue, true);
            }
        }

        public override void PersistDateTime(ref DateTime value) => value = _binaryReader.ReadDateTime();

        public override void PersistList<T>(List<T> value, PersistListItemCallback<T> callback)
        {
            var count = _binaryReader.ReadUInt16();

            for (var i = 0; i < count; i++)
            {
                var item = new T();

                callback(this, ref item);

                value.Add(item);
            }
        }

        public override unsafe void ReadBytesIntoStream(Stream destination, int numBytes)
        {
            const int bufferSize = 1024;
            Span<byte> buffer = stackalloc byte[bufferSize];

            var numBytesRemaining = numBytes;
            while (numBytesRemaining > 0)
            {
                var currentSpan = buffer.Slice(0, Math.Min(numBytesRemaining, bufferSize));

                var numBytesRead = _binaryReader.BaseStream.Read(currentSpan);
                destination.Write(currentSpan);

                numBytesRemaining -= numBytesRead;
            }
        }

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
                var unknown = _binaryReader.ReadByte();
                if (unknown != 0)
                {
                    throw new InvalidStateException($"Expected byte (index {i}) at position 0x{_binaryReader.BaseStream.Position - 1:X8} to be 0 but it was {unknown}");
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

        public override byte PersistVersion(byte maximumVersion)
        {
            _binaryWriter.Write(maximumVersion);
            return maximumVersion;
        }

        public override void PersistByte(ref byte value) => _binaryWriter.Write(value);

        public override void PersistInt16(ref short value) => _binaryWriter.Write(value);

        public override void PersistUInt16(ref ushort value) => _binaryWriter.Write(value);

        public override void PersistInt32(ref int value) => _binaryWriter.Write(value);

        public override void PersistUInt32(ref uint value) => _binaryWriter.Write(value);

        public override void PersistBoolean(ref bool value) => _binaryWriter.Write(value);

        public override void PersistAsciiString(ref string value) => _binaryWriter.WriteBytePrefixedAsciiString(value);

        public override void PersistUnicodeString(ref string value) => _binaryWriter.WriteBytePrefixedUnicodeString(value);

        public override void PersistSingle(ref float value) => _binaryWriter.Write(value);

        public override void PersistEnum<TEnum>(ref TEnum value) => _binaryWriter.WriteEnumAsUInt32(value);

        public override void PersistEnumByte<TEnum>(ref TEnum value) => _binaryWriter.WriteEnumAsByte(value);

        public override void PersistEnumFlags<TEnum>(ref TEnum value) => _binaryWriter.WriteEnumAsUInt32(value);

        public override void PersistEnumByteFlags<TEnum>(ref TEnum value) => _binaryWriter.WriteEnumAsByte(value);

        public override void PersistMatrix4x3(ref Matrix4x3 value, bool readVersion)
        {
            if (readVersion)
            {
                PersistVersion(1);
            }

            _binaryWriter.WriteMatrix4x3Transposed(value);
        }

        public override void PersistBitArray<TEnum>(ref BitArray<TEnum> result)
        {
            PersistVersion(1);

            result.SetAll(false);

            _binaryWriter.Write((uint)result.NumBitsSet);

            var valueToStringMap = Data.Ini.IniParser.GetEnumMapReverse<TEnum>();

            foreach (var setBit in result.GetSetBits())
            {
                var stringValue = valueToStringMap[setBit];

                _binaryWriter.WriteBytePrefixedAsciiString(stringValue);
            }
        }

        public override void PersistDateTime(ref DateTime value) => _binaryWriter.Write(value);

        public override void PersistList<T>(List<T> value, PersistListItemCallback<T> callback)
        {
            _binaryWriter.Write((ushort)value.Count);

            for (var i = 0; i < value.Count; i++)
            {
                var item = value[i];
                callback(this, ref item);
            }
        }

        public override void ReadBytesIntoStream(Stream destination, int numBytes)
        {
            throw new NotSupportedException();
        }

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

    public struct ObjectNameAndId
    {
        public string Name;
        public uint ObjectId;
    }

    public static class StatePersisterExtensions
    {
        public static void PersistObjectNameAndIdList(this StatePersister persister, List<ObjectNameAndId> value)
        {
            persister.PersistVersion(1);

            persister.PersistList(value, static (StatePersister persister, ref ObjectNameAndId item) =>
            {
                persister.PersistAsciiString(ref item.Name);
                persister.PersistObjectID(ref item.ObjectId);
            });
        }
    }
}
