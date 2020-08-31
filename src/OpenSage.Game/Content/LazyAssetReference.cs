using System;
using System.Diagnostics;

namespace OpenSage.Content
{
    [DebuggerDisplay("Value: {Value}")]
    public sealed class LazyAssetReference<T> : ILazyAssetReference
        where T : BaseAsset
    {
        private readonly Func<T> _getValue;
        private readonly T _value;

        // It seems like we could cache the result of _getValue, but then if the scope into which it was loaded gets disposed,
        // our local value would no longer be valid.
        // TODO: Still, we should cache it, and then clear the cache when the scope is popped.
        public T Value => _value ?? _getValue();

        BaseAsset ILazyAssetReference.Value => Value;

        public LazyAssetReference(Func<T> getValue)
        {
            _getValue = getValue;
        }

        public LazyAssetReference(T value)
        {
            _value = value;
        }
    }

    internal interface ILazyAssetReference
    {
        BaseAsset Value { get; }
    }
}
