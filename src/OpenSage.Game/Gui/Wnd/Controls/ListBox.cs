using System.Collections.Generic;

namespace OpenSage.Gui.Wnd.Controls
{
    public class ListBox : Control
    {
        public List<ListBoxItem> Items { get; } = new List<ListBoxItem>();

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                Invalidate();
            }
        }
    }

    public sealed class ListBoxItem
    {
        public object DataItem { get; set; }
        public string[] ColumnData { get; set; }
    }
}
