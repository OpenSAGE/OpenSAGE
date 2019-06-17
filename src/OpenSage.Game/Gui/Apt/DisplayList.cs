using System.Collections.Generic;

namespace OpenSage.Gui.Apt
{
    public sealed class DisplayList
    {
        public SortedDictionary<int, DisplayItem> Items { get; }

        public DisplayList()
        {
            Items = new SortedDictionary<int, DisplayItem>();
        }
    }
}
