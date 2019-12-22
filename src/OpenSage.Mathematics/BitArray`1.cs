using System;

namespace OpenSage.Mathematics
{
    public sealed class BitArray<TEnum> : IEquatable<BitArray<TEnum>>
        where TEnum : Enum
    {
        private BitArray512<TEnum> _data;
        
        public bool AnyBitSet => _data.AnyBitSet;
        public int NumBitsSet => _data.NumBitsSet;

        public BitArray() { }

        public BitArray(System.Collections.BitArray bitArray)
        {
            if (bitArray.Length >= 512)
            {
                throw new ArgumentException($"Cannot construct BitArray512 from a BitArray of length {bitArray.Length}.");
            }

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
            return _data.Get(bit);
        }

        public void Set(int bit, bool value)
        {
            _data.Set(bit, value);
        }

        public void Set(TEnum bit, bool value)
        {
            _data.Set(bit, value);
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

        public string DisplayName
        {
            get
            {
                var result = string.Empty;

                for (var i = 0; i < _data.Length; i++)
                {
                    if (_data.Get(i))
                    {
                        result += ((TEnum) (object) i).ToString() + ", ";
                    }
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
    }
}
