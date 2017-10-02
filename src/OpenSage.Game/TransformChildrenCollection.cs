using System;
using System.Collections.ObjectModel;

namespace OpenSage
{
    public sealed class TransformChildrenCollection : Collection<TransformComponent>
    {
        private readonly TransformComponent _transform;

        internal TransformChildrenCollection(TransformComponent transform)
        {
            _transform = transform;
        }

        private void AddItem(TransformComponent item)
        {
            if (item.ParentDirect != null)
            {
                throw new InvalidOperationException();
            }

            item.ClearCachedMatrices();
            item.ParentDirect = _transform;

            if (_transform.Game != null)
            {
                _transform.Game.AddComponentsRecursive(item.Entity);
            }
        }

        private void RemoveItem(TransformComponent item)
        {
            if (item.ParentDirect != _transform)
            {
                throw new InvalidOperationException();
            }

            if (_transform.Game != null)
            {
                _transform.Game.RemoveComponentsRecursive(item.Entity);
            }

            item.ClearCachedMatrices();
            item.ParentDirect = null;
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                RemoveItem(item);
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, TransformComponent item)
        {
            AddItem(item);

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            RemoveItem(this[index]);

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TransformComponent item)
        {
            AddItem(item);

            base.SetItem(index, item);
        }
    }
}
