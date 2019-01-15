using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
