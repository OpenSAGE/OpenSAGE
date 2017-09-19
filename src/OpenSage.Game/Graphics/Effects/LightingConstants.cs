using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Effects
{
    [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
    internal struct LightingConstants
    {
        public const int SizeInBytes = 160;

        [FieldOffset(0)]
        public Vector3 CameraPosition;

        [FieldOffset(16)]
        public Lights Lights;
    }

    [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
    public struct Lights
    {
        public const int SizeInBytes = 144;

        [FieldOffset(0)]
        public Light Light0;

        [FieldOffset(48)]
        public Light Light1;

        [FieldOffset(96)]
        public Light Light2;
    }
}
