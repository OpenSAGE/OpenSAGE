using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Gui.Apt
{
    public sealed class DisplayList
    {
        private Dictionary<int, IDisplayItem> _items;

        public Dictionary<int, IDisplayItem> Items => _items;

        public DisplayList()
        {
            _items = new Dictionary<int, IDisplayItem>();
        }
    }
}
