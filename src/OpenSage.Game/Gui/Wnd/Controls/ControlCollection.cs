using System;
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
            foreach (var child in this)
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
            foreach (var control in this)
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
            foreach (var child in control.Controls)
            {
                SetWindowRecursive(child, window);
            }
        }
    }
}
