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

        [FieldOffset(sizeof(Key))]
        public readonly ModifierKeys Modifiers;

        internal InputMessageValue(Key key, ModifierKeys modifiers)
        {
            MousePosition = Point2D.Zero;
            ScrollWheel = 0;
            Modifiers = modifiers;
            Key = key;
        }

        internal InputMessageValue(in Point2D mousePosition)
        {
            Key = 0;
            ScrollWheel = 0;
            Modifiers = 0;
            MousePosition = mousePosition;
        }

        internal InputMessageValue(int scrollWheel)
        {
            Key = 0;
            MousePosition = Point2D.Zero;
            Modifiers = 0;
            ScrollWheel = scrollWheel;
        }
    }
}
