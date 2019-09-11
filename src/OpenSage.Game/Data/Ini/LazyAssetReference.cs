using System;

namespace OpenSage.Data.Ini
{
    public sealed class LazyAssetReference<T>
        where T : class
    {
        private readonly Func<T> _getValue;

        // It seems like we could cache this, but then if the scope into which it was loaded gets disposed,
        // our local value would no longer be valid.
        public T Value => _getValue();

        public LazyAssetReference(Func<T> getValue)
        {
            _getValue = getValue;
        }
    }
}
