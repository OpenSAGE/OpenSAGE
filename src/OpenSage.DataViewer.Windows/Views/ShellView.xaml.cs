using System;
using System.Windows.Controls;
using System.Windows.Data;
using OpenSage.DataViewer.ViewModels;

namespace OpenSage.DataViewer.Views
{
    partial class ShellView
    {
        public ShellView()
        {
            InitializeComponent();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var sortedFiles = (CollectionViewSource) MainContent.FindResource("SortedFiles");
            sortedFiles.View.Refresh();
        }

        private void OnFilteringSortedFiles(object sender, FilterEventArgs e)
        {
            var fileEntry = (FileSystemEntryViewModel) e.Item;
            e.Accepted = fileEntry.FilePath.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
