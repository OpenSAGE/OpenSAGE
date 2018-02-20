using System;
using System.Collections.Generic;

namespace OpenSage.Utilities
{
    internal sealed class ResourcePool<T, TKey> : DisposableBase
        where T : class, IDisposable
        where TKey : struct
    {
        private readonly Func<TKey, T> _creator;

        private readonly Dictionary<TKey, List<T>> _available;
        private readonly Dictionary<TKey, List<T>> _leased;

        public ResourcePool(Func<TKey, T> creator)
        {
            _creator = creator;

            _available = new Dictionary<TKey, List<T>>();
            _leased = new Dictionary<TKey, List<T>>();
        }

        public T Acquire(in TKey key)
        {
            if (!_available.TryGetValue(key, out var available))
            {
                _available.Add(key, available = new List<T>());
            }

            T result;
            if (available.Count == 0)
            {
                result = AddDisposable(_creator(key));
            }
            else
            {
                result = available[available.Count - 1];
                available.RemoveAt(available.Count - 1);
            }

            if (!_leased.TryGetValue(key, out var leased))
            {
                _leased.Add(key, leased = new List<T>());
            }
            leased.Add(result);

            return result;
        }

        public void ReleaseAll()
        {
            foreach (var kvp in _leased)
            {
                if (!_available.TryGetValue(kvp.Key, out var available))
                {
                    throw new InvalidOperationException();
                }

                while (kvp.Value.Count > 0)
                {
                    var toRemove = kvp.Value[kvp.Value.Count - 1];
                    kvp.Value.RemoveAt(kvp.Value.Count - 1);

                    available.Add(toRemove);
                }
            }
        }
    }
}
