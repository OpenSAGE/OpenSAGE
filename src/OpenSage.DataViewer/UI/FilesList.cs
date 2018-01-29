using System;
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
            Rows.Add(_listBox);

            mainForm.InstallationChanged += (sender, e) =>
            {
                _fileSystem = e.FileSystem;

                RefreshItems();
            };
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
