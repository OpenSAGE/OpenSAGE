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
            MousePosition = Point2D.Zero;
            ScrollWheel = 0;

            Key = key;
        }

        internal InputMessageValue(in Point2D mousePosition)
        {
            Key = 0;
            ScrollWheel = 0;

            MousePosition = mousePosition;
        }

        internal InputMessageValue(int scrollWheel)
        {
            Key = 0;
            MousePosition = Point2D.Zero;

            ScrollWheel = scrollWheel;
        }
    }
}
