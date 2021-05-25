using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.FileFormats;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Mathematics;

namespace OpenSage.Data.Sav
{
    public sealed class SaveFileReader
    {
        private readonly BinaryReader _binaryReader;
        private readonly Stack<Segment> _segments;

        public BinaryReader Inner => _binaryReader;

        internal SaveFileReader(BinaryReader binaryReader)
        {
            _binaryReader = binaryReader;
            _segments = new Stack<Segment>();
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

        public byte ReadByte() => _binaryReader.ReadByte();

        public sbyte ReadSByte() => _binaryReader.ReadSByte();

        public ushort ReadUInt16() => _binaryReader.ReadUInt16();

        public int ReadInt32() => _binaryReader.ReadInt32();

        public uint ReadUInt32() => _binaryReader.ReadUInt32();

        public bool ReadBoolean() => _binaryReader.ReadBooleanChecked();

        public uint ReadObjectID() => ReadUInt32();

        public string ReadAsciiString() => _binaryReader.ReadBytePrefixedAsciiString();

        public string ReadUnicodeString() => _binaryReader.ReadBytePrefixedUnicodeString();

        public float ReadSingle() => _binaryReader.ReadSingle();

        public Vector2 ReadVector2() => _binaryReader.ReadVector2();

        public Vector3 ReadVector3() => _binaryReader.ReadVector3();

        public Point2D ReadPoint2D() => _binaryReader.ReadPoint2D();

        public TEnum ReadEnum<TEnum>()
            where TEnum : struct
        {
            return _binaryReader.ReadUInt32AsEnum<TEnum>();
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
            ReadVersion(1);

            var result = new BitArray<TEnum>();

            var stringToValueMap = Ini.IniParser.GetEnumMap<TEnum>();

            var count = ReadUInt32();
            for (var i = 0; i < count; i++)
            {
                var stringValue = ReadAsciiString();
                var enumValue = (TEnum)stringToValueMap[stringValue];
                result.Set(enumValue, true);
            }

            return result;
        }

        public ColorRgbF ReadColorRgbF() => _binaryReader.ReadColorRgbF();

        public ColorRgba ReadColorRgba() => _binaryReader.ReadColorRgba();

        public DateTime ReadDateTime() => _binaryReader.ReadDateTime();

        public RandomVariable ReadRandomVariable() => _binaryReader.ReadRandomVariable();

        public RandomAlphaKeyframe ReadRandomAlphaKeyframe() => RandomAlphaKeyframe.ReadFromSaveFile(_binaryReader);

        public RgbColorKeyframe ReadRgbColorKeyframe() => RgbColorKeyframe.ReadFromSaveFile(_binaryReader);

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

        public void ReadSpan<T>(Span<T> span)
            where T : unmanaged
        {
            var spanBytes = MemoryMarshal.Cast<T, byte>(span);
            _binaryReader.BaseStream.Read(spanBytes);
        }

        public uint BeginSegment()
        {
            var segmentLength = _binaryReader.ReadUInt32();

            var currentPosition = _binaryReader.BaseStream.Position;

            _segments.Push(new Segment(currentPosition, currentPosition + segmentLength));

            return segmentLength;
        }

        public void EndSegment()
        {
            var segment = _segments.Pop();

            if (_binaryReader.BaseStream.Position != segment.End)
            {
                Console.WriteLine("Skipped segment in .sav file");

                _binaryReader.BaseStream.Position = segment.End;

                //throw new InvalidDataException();
            }
        }

        private readonly struct Segment
        {
            public readonly long Start;
            public readonly long End;

            public Segment(long start, long end)
            {
                Start = start;
                End = end;
            }
        }

        public void __Skip(int numBytes)
        {
            _binaryReader.BaseStream.Position += numBytes;
        }
    }
}
