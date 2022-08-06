using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenSage.Mathematics
{
    /// <summary>
    /// A deterministic vector consisting of three <see cref="OpenSage.Mathematics.DFloat"/> values,
    /// with a <see cref="System.Numerics.Vector3"/> compatible memory layout.
    /// Calculations using this type should have exactly the same results on all runtimes and platforms.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DVector3 : IEquatable<DVector3>
    {
        public readonly DFloat X;
        public readonly DFloat Y;
        public readonly DFloat Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DVector3(DFloat x, DFloat y, DFloat z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DVector3(float x, float y, float z)
        {
            X = new DFloat(x);
            Y = new DFloat(y);
            Z = new DFloat(z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in DVector3 a, in DVector3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in DVector3 a, in DVector3 b)
        {
            return !(a == b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DVector3 other)
        {
            return this == other;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 operator +(in DVector3 a, in DVector3 b)
        {
            return new DVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 operator -(in DVector3 a, in DVector3 b)
        {
            return new DVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 operator *(in DVector3 vec, in DFloat c)
        {
            return new DVector3(vec.X * c, vec.Y * c, vec.Z * c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 operator *(in DFloat c, in DVector3 vec)
        {
            return vec * c;
        }

        public override bool Equals(object obj)
        {
            return obj is DVector3 f && this == f;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X.GetHashCode(), Y.GetHashCode(), Z.GetHashCode());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToVector3()
        {
            unsafe
            {
                fixed (DVector3* vecAddr = &this)
                {
                    return *(Vector3*) vecAddr;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DVector3 FromVector3(in Vector3 vector)
        {
            unsafe
            {
                fixed (Vector3* vecAddr = &vector)
                {
                    return *(DVector3*) vecAddr;
                }
            }
        }
    }
}
