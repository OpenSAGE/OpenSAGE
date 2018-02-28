using System;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;
using OpenSage.Mathematics;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class WndView : Splitter
    {
        public WndView(FileSystemEntry entry, Func<IntPtr, Game> createGame)
        {
            var treeItem = new TreeItem();            

            var treeView = new TreeView
            {
                Width = 400,
                DataStore = treeItem
            };

            var originalBorderColor = ColorRgbaF.Transparent;
            var originalBorderWidth = 1;

            Gui.Wnd.Controls.Control selectedElement = null;

            treeView.SelectionChanged += (sender, e) =>
            {
                if (selectedElement != null)
                {
                    selectedElement.BorderColor = originalBorderColor;
                    selectedElement.BorderWidth = originalBorderWidth;
                    selectedElement = null;
                }

                selectedElement = (Gui.Wnd.Controls.Control) ((TreeItem) treeView.SelectedItem).Tag;

                originalBorderColor = selectedElement.BorderColor;
                originalBorderWidth = selectedElement.BorderWidth;

                selectedElement.BorderColor = new ColorRgbaF(1f, 0.41f, 0.71f, 1);
                selectedElement.BorderWidth = 4;
            };

            Panel1 = treeView;

            Panel2 = new GameControl
            {
                CreateGame = h =>
                {
                    var game = createGame(h);

                    var window = game.ContentManager.Load<Gui.Wnd.Controls.Window>(entry.FilePath, new Content.LoadOptions { CacheAsset = false });
                    game.Scene2D.WndWindowManager.PushWindow(window);

                    treeItem.Children.Add(CreateTreeItemRecursive(window.Root));

                    return game;
                }
            };
        }

        private static TreeItem CreateTreeItemRecursive(Gui.Wnd.Controls.Control element)
        {
            var result = new TreeItem
            {
                Text = element.DisplayName,
                Expanded = true,
                Tag = element
            };

            foreach (var childElement in element.Controls)
            {
                result.Children.Add(CreateTreeItemRecursive(childElement));
            }

            return result;
        }
    }
}
