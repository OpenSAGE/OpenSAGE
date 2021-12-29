using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Content;
using OpenSage.FileFormats;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Mathematics;

namespace OpenSage
{
    public sealed class SaveFileReader
    {
        private readonly BinaryReader _binaryReader;
        private readonly Stack<Segment> _segments;

        public BinaryReader Inner => _binaryReader;

        public readonly SageGame SageGame;
        public readonly AssetStore AssetStore;

        internal SaveFileReader(BinaryReader binaryReader, Game game)
        {
            _binaryReader = binaryReader;
            _segments = new Stack<Segment>();

            SageGame = game.SageGame;
            AssetStore = game.AssetStore;
        }

        public byte ReadVersion(byte maximumVersion)
        {
            var result = _binaryReader.ReadByte();
            if (result == 0 || result > maximumVersion)
            {
                throw new InvalidDataException();
            }
            return result;
        }

        public void ReadByte(ref byte value)
        {
            value = _binaryReader.ReadByte();
        }

        public void ReadInt16(ref short value) => value = _binaryReader.ReadInt16();

        public void ReadUInt16(ref ushort value) => value = _binaryReader.ReadUInt16();

        public void ReadInt32(ref int value) => value = _binaryReader.ReadInt32();

        public uint ReadUInt32() => _binaryReader.ReadUInt32();

        public bool ReadBoolean() => _binaryReader.ReadBooleanChecked();

        public void ReadObjectID(ref uint value) => value = ReadUInt32();

        public void ReadFrame(ref uint value) => value = ReadUInt32();

        public void ReadAsciiString(ref string value) => value = _binaryReader.ReadBytePrefixedAsciiString();

        public string ReadUnicodeString() => _binaryReader.ReadBytePrefixedUnicodeString();

        public float ReadSingle() => _binaryReader.ReadSingle();

        public Vector2 ReadVector2() => _binaryReader.ReadVector2();

        public void ReadVector3(ref Vector3 value) => value = _binaryReader.ReadVector3();

        public Point2D ReadPoint2D() => _binaryReader.ReadPoint2D();

        public Point3D ReadPoint3D() => _binaryReader.ReadPoint3D();

        public void ReadEnum<TEnum>(ref TEnum value)
            where TEnum : struct
        {
            value = _binaryReader.ReadUInt32AsEnum<TEnum>();
        }

        public TEnum ReadEnumByte<TEnum>()
            where TEnum : struct
        {
            return _binaryReader.ReadByteAsEnum<TEnum>();
        }

        public TEnum ReadEnumFlags<TEnum>()
            where TEnum : struct
        {
            return _binaryReader.ReadUInt32AsEnumFlags<TEnum>();
        }

        public TEnum ReadEnumByteFlags<TEnum>()
            where TEnum : struct
        {
            return _binaryReader.ReadByteAsEnumFlags<TEnum>();
        }

        public Matrix4x3 ReadMatrix4x3(bool readVersion = true)
        {
            if (readVersion)
            {
                ReadVersion(1);
            }

            var m11 = _binaryReader.ReadSingle();
            var m21 = _binaryReader.ReadSingle();
            var m31 = _binaryReader.ReadSingle();
            var m41 = _binaryReader.ReadSingle();

            var m12 = _binaryReader.ReadSingle();
            var m22 = _binaryReader.ReadSingle();
            var m32 = _binaryReader.ReadSingle();
            var m42 = _binaryReader.ReadSingle();

            var m13 = _binaryReader.ReadSingle();
            var m23 = _binaryReader.ReadSingle();
            var m33 = _binaryReader.ReadSingle();
            var m43 = _binaryReader.ReadSingle();

            return new Matrix4x3(
                m11, m12, m13,
                m21, m22, m23,
                m31, m32, m33,
                m41, m42, m43);
        }

        public Matrix4x3 ReadMatrix4x3Transposed() => _binaryReader.ReadMatrix4x3Transposed();

        public BitArray<TEnum> ReadBitArray<TEnum>()
            where TEnum : Enum
        {
            var result = new BitArray<TEnum>();
            ReadBitArray(result);
            return result;
        }

        public void ReadBitArray<TEnum>(BitArray<TEnum> result)
            where TEnum : Enum
        {
            ReadVersion(1);

            var stringToValueMap = Data.Ini.IniParser.GetEnumMap<TEnum>();

            var count = ReadUInt32();
            for (var i = 0; i < count; i++)
            {
                var stringValue = "";
                ReadAsciiString(ref stringValue);

                var enumValue = (TEnum) stringToValueMap[stringValue];

                result.Set(enumValue, true);
            }
        }

        public ColorRgbF ReadColorRgbF() => _binaryReader.ReadColorRgbF();

        public ColorRgba ReadColorRgba() => _binaryReader.ReadColorRgba();

        public ColorRgba ReadColorRgbaInt() => _binaryReader.ReadColorRgbaInt();

        public DateTime ReadDateTime() => _binaryReader.ReadDateTime();

        public RandomVariable ReadRandomVariable() => _binaryReader.ReadRandomVariable();

        public RandomAlphaKeyframe ReadRandomAlphaKeyframe() => RandomAlphaKeyframe.ReadFromSaveFile(_binaryReader);

        public RgbColorKeyframe ReadRgbColorKeyframe() => RgbColorKeyframe.ReadFromSaveFile(_binaryReader);

        public void ReadObjectNameAndIdSet(List<ObjectNameAndId> set)
        {
            ReadVersion(1);

            var numItems = (ushort)set.Count;
            ReadUInt16(ref numItems);

            for (var j = 0; j < numItems; j++)
            {
                var objectNameAndId = new ObjectNameAndId();

                ReadAsciiString(ref objectNameAndId.Name);
                ReadObjectID(ref objectNameAndId.ObjectId);

                set.Add(objectNameAndId);
            }
        }

        public delegate T ReadListItemCallback<T>(SaveFileReader reader);

        public unsafe void ReadBytesIntoStream(Stream destination, int numBytes)
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

        public uint BeginSegment(string segmentName)
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

                _segments.Push(new Segment(currentPosition, segmentEnd, segmentName));

                return segmentLength;
            }
            else
            {
                var segmentLength = _binaryReader.ReadUInt32();

                var currentPosition = _binaryReader.BaseStream.Position;

                _segments.Push(new Segment(currentPosition, currentPosition + segmentLength, segmentName));

                return segmentLength;
            }
        }

        public void EndSegment()
        {
            var segment = _segments.Pop();

            if (_binaryReader.BaseStream.Position != segment.End)
            {
                throw new InvalidStateException($"Stream position expected to be at 0x{segment.End:X8} but was at 0x{_binaryReader.BaseStream.Position:X8} while reading {segment.Name}");
            }
        }

        private record struct Segment(long Start, long End, string Name);

        public void SkipUnknownBytes(int numBytes)
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
}
