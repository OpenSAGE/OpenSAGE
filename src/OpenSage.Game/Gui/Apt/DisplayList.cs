using System.Collections.Generic;

namespace OpenSage.Gui.Apt
{
    public sealed class DisplayList
    {
        public Dictionary<int, DisplayItem> Items { get; }

        public DisplayList()
        {
            Items = new Dictionary<int, DisplayItem>();
        }
    }
}
