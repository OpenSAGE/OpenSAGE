using System;

namespace OpenSage.Content
{
    public sealed class LazyAssetReference<T>
        where T : class
    {
        private readonly Func<T> _getValue;
        private readonly T _value;

        // It seems like we could cache the result of _getValue, but then if the scope into which it was loaded gets disposed,
        // our local value would no longer be valid.
        // TODO: Still, we should cache it, and then clear the cache when the scope is popped.
        public T Value => _value ?? _getValue();

        public LazyAssetReference(Func<T> getValue)
        {
            _getValue = getValue;
        }

        public LazyAssetReference(T value)
        {
            _value = value;
        }
    }
}
