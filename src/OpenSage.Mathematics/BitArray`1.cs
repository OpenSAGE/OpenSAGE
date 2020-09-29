using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OpenSage.Mathematics
{
    public sealed class BitArray<TEnum> : IEquatable<BitArray<TEnum>>
        where TEnum : Enum
    {
        private BitArray512 _data;

        public bool AnyBitSet => _data.AnyBitSet;
        public int NumBitsSet => _data.NumBitsSet;

        public BitArray()
        {
            var maxBits = GetNumValues();
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

        public BitArray(BitArray<TEnum> bitArray)
            : this()
        {
            for (var i = 0; i < _data.Length; i++)
            {
                _data.Set(i, bitArray._data.Get(i));
            }
        }

        public bool Get(int bit)
        {
            return _data.Get(bit);
        }

        public bool Get(TEnum bit)
        {
            // This avoids an object allocation.
            var bitI = Unsafe.As<TEnum, int>(ref bit);
            return _data.Get(bitI);
        }

        public void Set(int bit, bool value)
        {
            _data.Set(bit, value);
        }

        public void Set(TEnum bit, bool value)
        {
            // This avoids an object allocation.
            var bitI = Unsafe.As<TEnum, int>(ref bit);
            _data.Set(bitI, value);
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

        public bool Intersects(BitArray<TEnum> other)
        {
            return CountIntersectionBits(other) > 0;
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

        private static readonly Dictionary<Type, int> CachedNumValues = new Dictionary<Type, int>();

        private static int GetNumValues()
        {
            var key = typeof(TEnum);
            if (!CachedNumValues.TryGetValue(key, out var result))
            {
                result = Enum.GetValues(key).Length;
                CachedNumValues.Add(key, result);
            }
            return result;
        }
    }
}
