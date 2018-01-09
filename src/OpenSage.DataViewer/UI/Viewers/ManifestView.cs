using Eto.Forms;
using OpenSage.Data;
using OpenSage.Data.StreamFS;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class ManifestView : Splitter
    {
        public ManifestView(FileSystemEntry entry)
        {
            var gameStream = new GameStream(entry);

            var rootTreeItem = new TreeItem();

            var assetsTreeItem = new TreeItem { Text = "Assets" };
            rootTreeItem.Children.Add(assetsTreeItem);
            assetsTreeItem.Expanded = true;
            foreach (var asset in gameStream.ManifestFile.Assets)
            {
                assetsTreeItem.Children.Add(new TreeItem
                {
                    Text = asset.Name,
                    Tag = asset
                });
            }

            var treeView = new TreeView
            {
                Width = 200,
                DataStore = rootTreeItem
            };
            //listBox.SelectedValueChanged += (sender, e) => { };
            Panel1 = treeView;

            Panel2 = new Panel();
        }
    }
}
