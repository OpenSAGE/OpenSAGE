using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Gui.Apt
{
    public sealed class DisplayList
    {
        private SortedDictionary<int, DisplayItem> _items;
        private SortedDictionary<int, DisplayItem> _reverseItems;

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
            _items[depth] = item;
            _reverseItems[depth] = item;
        }

        public void RemoveItem(int depth)
        {
            _items.Remove(depth);
            _reverseItems.Remove(depth);
        }
    }
}
