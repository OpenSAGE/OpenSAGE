using System;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;
using OpenSage.Gui.Wnd;

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

            //Gui.Wnd.Controls.Control selectedElement = null;

            treeView.SelectionChanged += (sender, e) =>
            {
                //if (selectedElement != null)
                //{
                //    selectedElement.Highlighted = false;
                //    selectedElement = null;
                //}

                //selectedElement = (Gui.Wnd.Controls.Control) ((TreeItem) treeView.SelectedItem).Tag;
                //selectedElement.Highlighted = true;
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
