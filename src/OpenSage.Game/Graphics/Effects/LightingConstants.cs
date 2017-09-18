using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Effects
{
    [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
    public struct LightingConstants
    {
        public const int SizeInBytes = 160;

        [FieldOffset(0)]
        public Vector3 CameraPosition;

        [FieldOffset(16)]
        public Light Light0;

        [FieldOffset(64)]
        public Light Light1;

        [FieldOffset(112)]
        public Light Light2;
    }
}
