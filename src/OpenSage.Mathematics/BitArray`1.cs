using System;
using System.Collections.Generic;

namespace OpenSage.Mathematics
{
    public sealed class BitArray<TEnum> : IEquatable<BitArray<TEnum>>
        where TEnum : Enum
    {
        private BitArray512 _data;
        
        public bool AnyBitSet => _data.AnyBitSet;
        public int NumBitsSet => _data.NumBitsSet;

        public BitArray() {
            var maxBits = Enum.GetValues(typeof(TEnum)).Length;
            if (maxBits >= 512)
            {
                throw new Exception($"Cannot create a BitArray for enum {typeof(TEnum).Name}, because it has {maxBits} cases (max 512).");
            }
            _data = new BitArray512(maxBits);
        }

        public BitArray(System.Collections.BitArray bitArray)
        {
            if (bitArray.Length >= 512)
            {
                throw new ArgumentException($"Cannot construct BitArray512 from a BitArray of length {bitArray.Length}.");
            }

            _data = new BitArray512(bitArray.Length);

            for (var i = 0; i < bitArray.Length; i++)
            {
                _data.Set(i, bitArray[i]);
            }
        }

        public bool Get(int bit)
        {
            return _data.Get(bit);
        }

        public bool Get(TEnum bit)
        {
            return _data.Get((int) (object) bit);
        }

        public void Set(int bit, bool value)
        {
            _data.Set(bit, value);
        }

        public void Set(TEnum bit, bool value)
        {
            _data.Set((int) (object) bit, value);
        }

        public void SetAll(bool value)
        {
            _data.SetAll(value);
        }

        public void CopyFrom(BitArray<TEnum> other)
        {
            _data.CopyFrom(other._data);
        }

        public int CountIntersectionBits(BitArray<TEnum> other)
        {
            return _data.And(other._data).NumBitsSet;
        }

        public IEnumerable<TEnum> GetSetBits()
        {
            for (var i = 0; i < _data.Length; i++)
            {
                if (_data.Get(i))
                {
                    yield return (TEnum) (object) i;
                }
            }
        }

        public string DisplayName
        {
            get
            {
                var result = string.Empty;

                foreach (var bit in GetSetBits())
                {
                    result += bit.ToString() + ", ";
                }

                return (result == string.Empty)
                    ? "(None)"
                    : result.Trim(' ', ',');
            }
        }

        public bool Equals(BitArray<TEnum> other)
        {
            return _data.Equals(other);
        }

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }

        public BitArray<TEnum> Clone()
        {
            var result = new BitArray<TEnum>();
            result.CopyFrom(this);
            return result;
        }
    }
}
