using System.Runtime.InteropServices;

namespace OpenSage.Mathematics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Matrix4x3
    {
        public const int SizeInBytes = 48;

        public readonly float M11;
        public readonly float M21;
        public readonly float M31;
        public readonly float M41;

        public readonly float M12;
        public readonly float M22;
        public readonly float M32;
        public readonly float M42;

        public readonly float M13;
        public readonly float M23;
        public readonly float M33;
        public readonly float M43;

        public Matrix4x3(
            float m11, float m12, float m13,
            float m21, float m22, float m23,
            float m31, float m32, float m33,
            float m41, float m42, float m43)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M31 = m31;
            M32 = m32;
            M33 = m33;
            M41 = m41;
            M42 = m42;
            M43 = m43;
        }
    }
}
