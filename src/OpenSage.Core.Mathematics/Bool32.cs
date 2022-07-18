using System;

namespace OpenSage.Graphics.Mathematics
{
    public readonly struct Bool32 : IEquatable<Bool32>
    {
        private readonly int _value;

        public Bool32(bool val)
        {
            _value = val ? 1 : 0;
        }

        public override bool Equals(object obj)
        {
            return obj is Bool32 @bool && Equals(@bool);
        }

        public bool Equals(Bool32 other)
        {
            return _value == other._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static implicit operator bool(Bool32 d) => d._value == 1;

        public static implicit operator Bool32(bool d) => new Bool32(d);
    }
}
