using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Effects
{
    [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
    public struct Light
    {
        public const int SizeInBytes = 48;

        [FieldOffset(0)]
        public Vector3 Ambient;

        [FieldOffset(16)]
        public Vector3 Color;

        [FieldOffset(32)]
        public Vector3 Direction;
    };
}
