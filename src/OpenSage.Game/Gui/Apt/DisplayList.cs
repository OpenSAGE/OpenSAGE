using System.Collections.Generic;

namespace OpenSage.Gui.Apt
{
    public sealed class DisplayList
    {
        public Dictionary<int, IDisplayItem> Items { get; }

        public DisplayList()
        {
            Items = new Dictionary<int, IDisplayItem>();
        }
    }
}
