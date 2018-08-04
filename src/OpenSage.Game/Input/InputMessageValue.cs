using System.Runtime.InteropServices;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Input
{
    [StructLayout(LayoutKind.Explicit)]
    public struct InputMessageValue
    {
        [FieldOffset(0)]
        public Key Key;

        [FieldOffset(8)]
        public ModifierKeys Modifier;

        [FieldOffset(0)]
        public Point2D MousePosition;

        [FieldOffset(0)]
        public int ScrollWheel;
    }
}
