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

            WndWindow selectedElement = null;

            treeView.SelectionChanged += (sender, e) =>
            {
                if (selectedElement != null)
                {
                    selectedElement.Highlighted = false;
                    selectedElement = null;
                }

                selectedElement = (WndWindow) ((TreeItem) treeView.SelectedItem).Tag;
                selectedElement.Highlighted = true;
            };

            Panel1 = treeView;

            Panel2 = new GameControl
            {
                CreateGame = h =>
                {
                    var game = createGame(h);

                    var window = game.ContentManager.Load<WndTopLevelWindow>(entry.FilePath);
                    game.Scene2D.WndWindowManager.PushWindow(window);

                    treeItem.Children.Add(CreateTreeItemRecursive(window.Root));

                    return game;
                }
            };
        }

        private static TreeItem CreateTreeItemRecursive(WndWindow element)
        {
            var result = new TreeItem
            {
                Text = element.DisplayName,
                Expanded = true,
                Tag = element
            };

            foreach (var childElement in element.Children)
            {
                result.Children.Add(CreateTreeItemRecursive(childElement));
            }

            return result;
        }
    }
}
