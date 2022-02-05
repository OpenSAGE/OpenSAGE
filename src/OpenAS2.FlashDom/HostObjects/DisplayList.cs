using System;
using System.Collections.Generic;

namespace OpenAS2.HostObjects
{
    public sealed class DisplayList : DisposableBase
    {
        private readonly SortedDictionary<int, DisplayItem> _items;

        // should we replace this with LINQ or a specially designated IEnumerable?
        private readonly SortedDictionary<int, DisplayItem> _reverseItems;

        public IReadOnlyDictionary<int, DisplayItem> Items => _items;
        public IReadOnlyDictionary<int, DisplayItem> ReverseItems => _reverseItems; 

        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }

        public DisplayList()
        {
            _items = new SortedDictionary<int, DisplayItem>();
            _reverseItems = new SortedDictionary<int, DisplayItem>(new DescendingComparer<int>());
        }

        public void AddItem(int depth, DisplayItem item)
        {
            if (_items.TryGetValue(depth, out var previous))
            {
                RemoveAndDispose(ref previous);
            }
            _items[depth] = AddDisposable(item);
            _reverseItems[depth] = item;
        }

        public void RemoveItem(int depth)
        {
            if (!_items.TryGetValue(depth, out var previous))
            {
                return;
            }
            RemoveAndDispose(ref previous);
            _items.Remove(depth);
            _reverseItems.Remove(depth);
        }
    }
}
