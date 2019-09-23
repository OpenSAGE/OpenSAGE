using System;
using System.Collections;
using System.Collections.Generic;
using OpenSage.Content.Loaders;

namespace OpenSage.Content
{
    public sealed class ScopedAssetCollection<TKey, TValue> : IScopedAssetCollection, IEnumerable<TValue>
        where TValue : class
    {
        private sealed class AssetScope
        {
            public readonly Dictionary<TKey, TValue> Assets = new Dictionary<TKey, TValue>();
            public readonly int StartId;

            public AssetScope(int startId)
            {
                StartId = startId;
            }
        }

        private readonly Stack<AssetScope> _assetScopes = new Stack<AssetScope>();

        private readonly Dictionary<int, TValue> _byId = new Dictionary<int, TValue>();
        private readonly Dictionary<TValue, int> _byValue = new Dictionary<TValue, int>();

        private readonly AssetStore _assetStore;
        private readonly Func<TValue, TKey> _getKey;
        private readonly Func<TKey, TKey> _normalizeKey;
        private readonly IOnDemandAssetLoader<TKey, TValue> _loader;
        private readonly bool _disposeAssets;

        private int _nextId = 1;

        public int Count => _byId.Count;

        internal ScopedAssetCollection(
            AssetStore assetStore,
            Func<TValue, TKey> getKey,
            Func<TKey, TKey> normalizeKey = null,
            IOnDemandAssetLoader<TKey, TValue> loader = null)
        {
            _assetStore = assetStore;
            _getKey = getKey;
            _normalizeKey = normalizeKey;
            _loader = loader;

            // TODO: Maybe all assets should implement IDisposable?
            _disposeAssets = typeof(IDisposable).IsAssignableFrom(typeof(TValue));
        }

        void IScopedAssetCollection.PushScope()
        {
            _assetScopes.Push(new AssetScope(_nextId));
        }

        void IScopedAssetCollection.PopScope()
        {
            var assetScope = _assetScopes.Pop();
            foreach (var asset in assetScope.Assets.Values)
            {
                if (_disposeAssets)
                {
                    ((IDisposable) asset).Dispose();
                }
                _byValue.Remove(asset);
            }
            for (var i = assetScope.StartId; i < _nextId; i++)
            {
                _byId.Remove(i);
            }
        }

        public TValue GetByKey(TKey key)
        {
            var normalizedKey = (_normalizeKey != null)
                ? _normalizeKey(key)
                : key;

            // Find existing cached item.
            foreach (var assetScope in _assetScopes)
            {
                if (assetScope.Assets.TryGetValue(normalizedKey, out var result))
                {
                    return result;
                }
            }

            // If we can, create new item and cache it.
            if (_loader != null)
            {
                var newValue = _loader.Load(normalizedKey, _assetStore.LoadContext);
                _assetScopes.Peek().Assets.Add(normalizedKey, newValue);
                return newValue;
            }

            return null;
        }

        public TValue GetByInternalId(int internalId)
        {
            return _byId[internalId];
        }

        // TODO: Remove this, when we store internalId in each asset.
        public int GetInternalId(TValue value)
        {
            return _byValue[value];
        }

        internal void Add(TValue asset)
        {
            // Existing entries take precedence
            var assetScope = _assetScopes.Peek();
            var key = _getKey(asset);
            if (!assetScope.Assets.ContainsKey(key))
            {
                assetScope.Assets.Add(key, asset);

                _byId.Add(_nextId, asset);
                _byValue.Add(asset, _nextId);

                _nextId++;
            }
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            foreach (var assetScope in _assetScopes)
            {
                foreach (var asset in assetScope.Assets.Values)
                {
                    yield return asset;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TValue>) this).GetEnumerator();
    }
}
