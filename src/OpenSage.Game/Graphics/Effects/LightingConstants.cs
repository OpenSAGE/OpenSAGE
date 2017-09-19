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
}
