using System;
using Eto.Forms;

namespace OpenSage.DataViewer.UI.Viewers.Ini
{
    public sealed class IniEntry
    {
        private readonly Func<Control> _createView;

        public string Name { get; }

        public IniEntry(string name, Func<Control> createView)
        {
            Name = name;

            _createView = createView;
        }

        public Control CreateView() => _createView();
    }
}
