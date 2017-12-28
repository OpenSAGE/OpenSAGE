using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Elements;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class WndView : Splitter
    {
        public WndView(FileSystemEntry entry, Game game)
        {
            var wndComponent = new WndComponent
            {
                Window = game.ContentManager.Load<GuiWindow>(entry.FilePath)
            };

            var scene = new Scene();

            var entity = new Entity();
            entity.Components.Add(wndComponent);
            scene.Entities.Add(entity);

            game.Scene = scene;

            var treeItem = new TreeItem();
            treeItem.Children.Add(CreateTreeItemRecursive(wndComponent.Window.Root));

            var treeView = new TreeView
            {
                Width = 400,
                DataStore = treeItem
            };

            UIElement selectedElement = null;

            treeView.SelectionChanged += (sender, e) =>
            {
                if (selectedElement != null)
                {
                    selectedElement.Highlighted = false;
                    selectedElement = null;
                }

                selectedElement = (UIElement) ((TreeItem) treeView.SelectedItem).Tag;
                selectedElement.Highlighted = true;
            };

            Panel1 = treeView;

            Panel2 = new GameControl
            {
                Game = game
            };
        }

        private static TreeItem CreateTreeItemRecursive(UIElement element)
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
