using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

using Representation = System.Decimal;

namespace OpenSage.Mathematics
{
    public readonly struct Fixed : IEquatable<Fixed>
    {
        private readonly Representation _value;

        public Fixed(Representation value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Fixed a, Fixed b)
        {
            return a._value == b._value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Fixed a, Fixed b)
        {
            return !(a == b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Fixed other)
        {
            return this == other;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed operator +(Fixed a, Fixed b)
        {
            return new Fixed(a._value + b._value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed operator -(Fixed a, Fixed b)
        {
            return new Fixed(a._value - b._value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed operator *(Fixed a, Fixed b)
        {
            return new Fixed(a._value * b._value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed operator /(Fixed a, Fixed b)
        {
            return new Fixed(a._value / b._value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Fixed a, Fixed b)
        {
            return a._value < b._value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Fixed a, Fixed b)
        {
            return a._value > b._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Fixed f ? _value == f._value : false;
        }

        public override string ToString()
        {
            return _value.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }
    }

    public readonly struct Fixed3 : IEquatable<Fixed3>
    {
        public readonly Fixed X;
        public readonly Fixed Y;
        public readonly Fixed Z;

        public Fixed3(Fixed x, Fixed y, Fixed z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Fixed3 a, Fixed3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Fixed3 a, Fixed3 b)
        {
            return !(a == b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Fixed3 other)
        {
            return this == other;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3 operator +(Fixed3 a, Fixed3 b)
        {
            return new Fixed3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3 operator -(Fixed3 a, Fixed3 b)
        {
            return new Fixed3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3 operator *(Fixed3 vec, Fixed c)
        {
            return new Fixed3(vec.X * c, vec.Y * c, vec.Z * c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3 operator /(Fixed3 vec, Fixed c)
        {
            return new Fixed3(vec.X / c, vec.Y / c, vec.Z / c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed3 operator *(Fixed c, Fixed3 vec)
        {
            return vec * c;
        }

        public override bool Equals(object obj)
        {
            return obj is Fixed3 f ? this == f : false;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public override int GetHashCode()
        {
            var hashCode = -307843816;
            hashCode = hashCode * -1521134295 + EqualityComparer<Fixed>.Default.GetHashCode(X);
            hashCode = hashCode * -1521134295 + EqualityComparer<Fixed>.Default.GetHashCode(Y);
            hashCode = hashCode * -1521134295 + EqualityComparer<Fixed>.Default.GetHashCode(Z);
            return hashCode;
        }
    }
}
