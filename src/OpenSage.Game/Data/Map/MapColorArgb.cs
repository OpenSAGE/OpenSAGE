﻿using System;
using System.IO;

namespace OpenSage.Data.Map
{
    public struct MapColorArgb
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;

        public MapColorArgb(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        internal static MapColorArgb Parse(BinaryReader reader)
        {
            var value = reader.ReadUInt32();
            return new MapColorArgb
            {
                A = (byte) ((value >> 24) & 0xFF),
                R = (byte) ((value >> 16) & 0xFF),
                G = (byte) ((value >> 8) & 0xFF),
                B = (byte) (value & 0xFF)
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            var combined = (A << 24) | (R << 16) | (G << 8) | B;
            writer.Write((uint) combined);
        }

        public override bool Equals(object obj)
        {
            return (obj is MapColorArgb) && Equals((MapColorArgb) obj);
        }

        public bool Equals(MapColorArgb other)
        {
            return A == other.A
                && R == other.R
                && G == other.G
                && B == other.B;
        }

        public override string ToString()
        {
            return $"{{A:{A} R:{R} G:{G} B:{B}}}";
        }

        public override int GetHashCode() => HashCode.Combine(A, R, G, B);
    }
}
