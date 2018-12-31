using System;
using System.Globalization;
using System.Runtime.CompilerServices;

using Representation = OpenSage.Mathematics.SoftFloat;

namespace OpenSage.Mathematics
{
    // TODO: This is just a wrapper around SoftFloat. Is this necessary?
    public readonly struct DFloat : IEquatable<DFloat>
    {
        private readonly Representation _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DFloat(Representation value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DFloat(float value)
        {
            _value = (Representation) value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(DFloat a, DFloat b)
        {
            return a._value == b._value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(DFloat a, DFloat b)
        {
            return !(a == b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DFloat other)
        {
            return this == other;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DFloat operator +(DFloat a, DFloat b)
        {
            return new DFloat(a._value + b._value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DFloat operator -(DFloat a, DFloat b)
        {
            return new DFloat(a._value - b._value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DFloat operator *(DFloat a, DFloat b)
        {
            return new DFloat(a._value * b._value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(DFloat a, DFloat b)
        {
            return a._value < b._value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(DFloat a, DFloat b)
        {
            return a._value > b._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is DFloat f ? _value == f._value : false;
        }

        public override string ToString()
        {
            return _value.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }
    }
}
