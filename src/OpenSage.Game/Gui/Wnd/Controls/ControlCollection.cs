using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenSage.Gui.Wnd.Controls
{
    public sealed class ControlCollection : Collection<Control>
    {
        private readonly Control _owner;

        public ControlCollection(Control owner)
        {
            _owner = owner;
        }

        public Control FindControl(string name, bool searchAllChildren = true)
        {
            foreach (var child in AsList())
            {
                if (child.Name == name)
                {
                    return child;
                }

                if (searchAllChildren)
                {
                    var foundChild = child.Controls.FindControl(name, searchAllChildren);
                    if (foundChild != null)
                    {
                        return foundChild;
                    }
                }
            }

            return null;
        }

        protected override void ClearItems()
        {
            foreach (var control in AsList())
            {
                control.ParentInternal = null;
                SetWindowRecursive(control, null);
            }

            base.ClearItems();

            _owner.InvalidateLayout();
        }

        protected override void InsertItem(int index, Control item)
        {
            if (item.ParentInternal != null)
            {
                throw new InvalidOperationException();
            }

            item.ParentInternal = _owner;
            SetWindowRecursive(item, _owner.Window);

            base.InsertItem(index, item);

            item.InvalidateLayout();
            _owner.InvalidateLayout();
        }

        protected override void RemoveItem(int index)
        {
            this[index].ParentInternal = null;
            SetWindowRecursive(this[index], null);

            base.RemoveItem(index);

            _owner.InvalidateLayout();
        }

        protected override void SetItem(int index, Control item)
        {
            if (item.ParentInternal != null)
            {
                throw new InvalidOperationException();
            }

            item.ParentInternal = _owner;
            SetWindowRecursive(item, _owner.Window);

            base.SetItem(index, item);

            item.InvalidateLayout();
            _owner.InvalidateLayout();
        }

        internal static void SetWindowRecursive(Control control, Window window)
        {
            control.Window = window;
            foreach (var child in control.Controls.AsList())
            {
                SetWindowRecursive(child, window);
            }
        }

        // Obtains a reference to the internal List<T>.
        // Unlike Collection<T>, List<T> supports efficient zero-allocation enumeration.
        // ControlCollection is enumerated so much that the cost iteration has a measurable effect on memory usage.
        // Do NOT refactor this to return an abstract type, as that will force the enumerator to be boxed, resulting in allocations.
        public List<Control> AsList()
        {
            return Items as List<Control>;
        }

        public new IEnumerator<Control> GetEnumerator()
        {
            throw new InvalidOperationException($"Do not enumerate ControlCollection directly - use the {nameof(AsList)} method.");
        }
    }
}
