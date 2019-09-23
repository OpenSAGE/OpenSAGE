using System;
using System.Collections;

namespace OpenSage.Mathematics
{
    // TODO: Don't use .NET's BitArray as the internal implementation.
    public sealed class BitArray<TEnum> : IEquatable<BitArray>
        where TEnum : struct
    {
        private static readonly int NumValues = Enum.GetValues(typeof(TEnum)).Length;

        private readonly BitArray _inner;

        public bool AnyBitSet
        {
            get
            {
                for (var i = 0; i < _inner.Count; i++)
                {
                    if (_inner[i])
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public int NumBitsSet
        {
            get
            {
                var result = 0;

                for (var i = 0; i < _inner.Count; i++)
                {
                    if (_inner[i])
                    {
                        result += 1;
                    }
                }

                return result;
            }
        }

        public BitArray()
        {
            _inner = new BitArray(NumValues);
        }

        private BitArray(BitArray inner)
        {
            _inner = inner;
        }

        public bool Get(TEnum index)
        {
            return _inner.Get((int) (object) index);
        }

        public void SetAll(bool value)
        {
            _inner.SetAll(value);
        }

        public void Set(TEnum index, bool value)
        {
            _inner.Set((int) (object) index, value);
        }

        public BitArray<TEnum> And(BitArray<TEnum> value)
        {
            // TODO: This is slow.
            return new BitArray<TEnum>(((BitArray) _inner.Clone()).And(value._inner));
        }

        public override bool Equals(object obj)
        {
            if (obj is BitArray<TEnum> x)
            {
                for (var i = 0; i < _inner.Count; i++)
                {
                    if (_inner[i] != x._inner[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_inner.Count);
        }

        bool IEquatable<BitArray>.Equals(BitArray other)
        {
            return Equals(other);
        }

        public string DisplayName
        {
            get
            {
                var result = string.Empty;

                for (var i = 0; i < _inner.Count; i++)
                {
                    if (_inner[i])
                    {
                        result += ((TEnum) (object) i).ToString() + ", ";
                    }
                }

                return (result == string.Empty)
                    ? "(None)"
                    : result.Trim(' ', ',');
            }
        }
    }
}
