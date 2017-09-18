using System;
using System.Collections;

namespace OpenSage.Data.Ini
{
    public sealed class BitArray<TEnum>
        where TEnum : struct
    {
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
            _inner = new BitArray(Enum.GetValues(typeof(TEnum)).Length);
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
    }
}
