using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenSage.Data.Ini
{
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

        public BitArray()
        {
            _inner = new BitArray(NumValues);
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

        public override bool Equals(object obj)
        {
            return obj is BitArray<TEnum> x && _inner.Equals(x._inner);
        }

        public override int GetHashCode()
        {
            return -311176850 + EqualityComparer<BitArray>.Default.GetHashCode(_inner);
        }
    }
}
