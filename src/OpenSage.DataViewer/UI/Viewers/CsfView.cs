using System;
using System.Linq;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.Data.Csf;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class CsfView : GridView
    {
        private readonly FileSystemEntry _entry;

        public CsfView(FileSystemEntry entry)
        {
            _entry = entry;

            Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell { Binding = Binding.Property((CsfFileEntry r) => r.Name) },
                HeaderText = "Name"
            });

            Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell { Binding = Binding.Property((CsfFileEntry r) => r.Value) },
                HeaderText = "Value"
            });

            Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell { Binding = Binding.Property((CsfFileEntry r) => r.ExtraVaue) },
                HeaderText = "Extra Value"
            });

            var csfFile = CsfFile.FromFileSystemEntry(_entry);

            DataStore = csfFile.Labels
                .Select(x => new CsfFileEntry(x))
                .ToList();
        }

        protected override void OnPreLoad(EventArgs e)
        {
            

            base.OnPreLoad(e);
        }

        private sealed class CsfFileEntry
        {
            public string Name { get; }
            public string Value { get; }
            public string ExtraVaue { get; }

            public CsfFileEntry(CsfLabel label)
            {
                Name = label.Name;

                if (label.Strings.Length > 0)
                {
                    var csfString = label.Strings[0];
                    Value = csfString.Value;
                    ExtraVaue = csfString.ExtraValue;
                }
            }
        }
    }
}
