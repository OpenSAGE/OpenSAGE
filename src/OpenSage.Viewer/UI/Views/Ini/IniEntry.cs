using System;

namespace OpenSage.Viewer.UI.Views.Ini
{
    internal sealed class IniEntry
    {
        private readonly Func<AssetView> _createView;

        public string Name { get; }

        public IniEntry(string name, Func<AssetView> createView)
        {
            Name = name;

            _createView = createView;
        }

        public AssetView CreateView() => _createView();
    }
}
