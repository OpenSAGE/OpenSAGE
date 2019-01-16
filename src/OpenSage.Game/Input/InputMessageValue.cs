using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Input
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct InputMessageValue
    {
        [FieldOffset(0)]
        public readonly Key Key;

        [FieldOffset(0)]
        public readonly Point2D MousePosition;

        [FieldOffset(0)]
        public readonly int ScrollWheel;

        internal InputMessageValue(Key key)
        {
            Key = key;
            MousePosition = Point2D.Zero;
            ScrollWheel = 0;
        }

        internal InputMessageValue(in Point2D mousePosition)
        {
            Key = 0;
            MousePosition = mousePosition;
            ScrollWheel = 0;
        }

        internal InputMessageValue(int scrollWheel)
        {
            Key = 0;
            MousePosition = Point2D.Zero;
            ScrollWheel = scrollWheel;
        }
    }
}
