using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenSage.Mathematics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Matrix4x3 : IEquatable<Matrix4x3>
    {
        public const int SizeInBytes = 48;

        public static readonly Matrix4x3 Identity = new Matrix4x3(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1,
            0, 0, 0);

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

        public override bool Equals(object obj)
        {
            return obj is Matrix4x3 x && Equals(x);
        }

        public bool Equals(Matrix4x3 other)
        {
            return M11 == other.M11 &&
                   M21 == other.M21 &&
                   M31 == other.M31 &&
                   M41 == other.M41 &&
                   M12 == other.M12 &&
                   M22 == other.M22 &&
                   M32 == other.M32 &&
                   M42 == other.M42 &&
                   M13 == other.M13 &&
                   M23 == other.M23 &&
                   M33 == other.M33 &&
                   M43 == other.M43;
        }

        public Matrix4x4 ToMatrix4x4()
        {
            return new Matrix4x4(
                M11, M12, M13, 0,
                M21, M22, M23, 0,
                M31, M32, M33, 0,
                M41, M42, M43, 1);
        }

        public static bool operator ==(in Matrix4x3 left, in Matrix4x3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in Matrix4x3 left, in Matrix4x3 right)
        {
            return !(left == right);
        }
    }
}
