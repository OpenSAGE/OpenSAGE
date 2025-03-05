using System.Runtime.InteropServices;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Input
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct InputMessageValue
    {
        // Keyboard-related values
        [FieldOffset(0)]
        public readonly Key Key;

        [FieldOffset(sizeof(Key))]
        public readonly ModifierKeys Modifiers;

        // Mouse-related values
        [FieldOffset(0)]
        public readonly Point2D MousePosition;

        [FieldOffset(sizeof(int) * 2)] // Size of Point2D, because sizeof(Point2D) doesn't work
        public readonly int ScrollWheel;

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

        internal InputMessageValue(int scrollWheel, in Point2D mousePosition)
        {
            Key = 0;
            Modifiers = 0;
            MousePosition = mousePosition;
            ScrollWheel = scrollWheel;
        }
    }
}
