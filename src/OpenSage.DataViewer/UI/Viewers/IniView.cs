using System;
using System.Collections.Generic;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.UI.Viewers.Ini;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class IniView : Splitter
    {
        private readonly ListBox _listBox;

        public IniView(FileSystemEntry entry, Game game)
        {
            var iniDataContext = new IniDataContext(entry.FileSystem);
            iniDataContext.LoadIniFile(entry);

            var subObjects = new List<IniEntry>();

            foreach (var objectDefinition in iniDataContext.Objects)
            {
                subObjects.Add(new IniEntry(objectDefinition.Name, () => new ObjectDefinitionView(game, objectDefinition)));
            }

            foreach (var particleSystem in iniDataContext.ParticleSystems)
            {
                subObjects.Add(new IniEntry(particleSystem.Name, () => new ParticleSystemView(game, particleSystem)));
            }

            _listBox = new ListBox();
            _listBox.Width = 200;
            _listBox.ItemTextBinding = Binding.Property((IniEntry v) => v.Name);
            _listBox.SelectedValueChanged += OnSelectedValueChanged;
            _listBox.DataStore = subObjects;
            Panel1 = _listBox;

            Panel2 = new Panel();
        }

        private void OnSelectedValueChanged(object sender, EventArgs e)
        {
            if (Panel2 != null)
            {
                var existingView = Panel2;
                Panel2 = null;
                existingView.Dispose();
            }

            if (_listBox.SelectedValue != null)
            {
                Panel2 = ((IniEntry) _listBox.SelectedValue).CreateView();
            }
        }
    }
}
