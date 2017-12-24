using System;
using System.Linq;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.Data.Apt;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class ConstViewer : GridView
    {
        private readonly FileSystemEntry _entry;

        public ConstViewer(FileSystemEntry entry)
        {
            _entry = entry;

            Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell { Binding = Binding.Property((ConstFileEntry r) => r.Index) },
                HeaderText = "Index"
            });

            Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell { Binding = Binding.Property((ConstFileEntry r) => r.Value) },
                HeaderText = "Value"
            });


            var constFile = ConstantData.FromFileSystemEntry(_entry);

            DataStore = constFile.Entries
                .Select((e,i) => new ConstFileEntry(e,i))
                .ToList();
        }

        protected override void OnPreLoad(EventArgs e)
        {


            base.OnPreLoad(e);
        }

        private sealed class ConstFileEntry
        {
            public string Index { get; }
            public string Value { get; }

            public ConstFileEntry(ConstantEntry entry,int index)
            {
                Index = index.ToString();

                if (entry.Type == ConstantEntryType.Number)
                    Value = Convert.ToUInt32(entry.Value).ToString();
                else
                    Value = Convert.ToString(entry.Value);
            }
        }
    }
}
