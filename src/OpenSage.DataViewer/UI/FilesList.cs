using System;
using System.IO;
using System.Linq;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Framework;

namespace OpenSage.DataViewer.UI
{
    public sealed class FilesList : TableLayout
    {
        public event EventHandler<FileSystemEntryEventArgs> SelectedFileChanged;

        private readonly SearchBox _searchBox;
        private readonly ListBox _listBox;

        private FileSystem _fileSystem;

        public FilesList(MainForm mainForm)
        {
            _searchBox = new SearchBox();
            _searchBox.PlaceholderText = "Search";
            _searchBox.TextChanged += (sender, e) => RefreshItems();
            Rows.Add(_searchBox);

            _listBox = new ListBox();
            _listBox.ItemTextBinding = Binding.Property((FileSystemEntry e) => e.FilePath);
            _listBox.SelectedValueChanged += OnSelectedValueChanged;
            _listBox.ContextMenu = new ContextMenu(new ButtonMenuItem(ExportSelectedItem)
            {
                Text = "Export..."
            });

            Rows.Add(_listBox);

            mainForm.InstallationChanged += (sender, e) =>
            {
                _fileSystem = e.FileSystem;

                RefreshItems();
            };
        }

        private void ExportSelectedItem(object sender, EventArgs args)
        {
            var entry = (FileSystemEntry) _listBox.SelectedValue;

            var saveDialog = new SaveFileDialog();
            saveDialog.FileName = Path.GetFileName(entry.FilePath);

            var extension = Path.GetExtension(entry.FilePath);
            saveDialog.Filters.Add(new FileDialogFilter($"{extension} Files", extension));
            saveDialog.Filters.Add(new FileDialogFilter("All Files", "*.*"));

            var result = saveDialog.ShowDialog(this);

            if (result != DialogResult.Ok)
            {
                return;
            }

            using (var entryStream = entry.Open())
            {
                using (var fileStream = File.OpenWrite(saveDialog.FileName))
                {
                    entryStream.CopyTo(fileStream);
                }
            }
        }

        private void RefreshItems()
        {
            _listBox.DataStore = _fileSystem.Files
                .Where(x => string.IsNullOrEmpty(_searchBox.Text) || x.FilePath.IndexOf(_searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(x => x.FilePath)
                .ToList();
        }

        private void OnSelectedValueChanged(object sender, EventArgs e)
        {
            SelectedFileChanged?.Invoke(this, new FileSystemEntryEventArgs((FileSystemEntry) _listBox.SelectedValue));
        }
    }
}
