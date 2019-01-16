using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Shaders
{
    internal static class GlobalLightingTypes
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
        }

        [StructLayout(LayoutKind.Sequential, Size = SizeInBytes)]
        public struct LightingConstantsVS
        {
            public const int SizeInBytes = 64;

            public Matrix4x4 CloudShadowMatrix;
        }

        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        public struct LightingConstantsPS
        {
            public const int SizeInBytes = 48 * 3;

            [FieldOffset(0)]
            public Light Light0;

            [FieldOffset(48)]
            public Light Light1;

            [FieldOffset(96)]
            public Light Light2;
        }
    }
}
