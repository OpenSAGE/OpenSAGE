using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenSage.Data.Ini
{
    public sealed class BitArrayEqualityComparer<TEnum> : IEqualityComparer<BitArray<TEnum>>
        where TEnum : struct
    {
        public bool Equals(BitArray<TEnum> x, BitArray<TEnum> y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(BitArray<TEnum> obj)
        {
            return obj.GetHashCode();
        }
    }

    public sealed class BitArray<TEnum>
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
            return -311176850 + _inner.Count;
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
