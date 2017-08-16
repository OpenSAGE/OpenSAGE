using System;
using System.Collections;

namespace OpenZH.Data.Ini
{
    public sealed class BitArray<TEnum>
        where TEnum : struct
    {
        private readonly BitArray _inner;

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
    }
}
