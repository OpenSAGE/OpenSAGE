using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;
using OpenSage.Gui;
using OpenSage.Gui.Elements;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class WndView : Splitter
    {
        public WndView(FileSystemEntry entry, Game game)
        {
            var guiComponent = new GuiComponent
            {
                RootWindow = game.ContentManager.Load<UIElement>(entry.FilePath)
            };

            var scene = new Scene();

            var entity = new Entity();
            entity.Components.Add(guiComponent);
            scene.Entities.Add(entity);

            game.Scene = scene;

            var treeItem = new TreeItem();
            treeItem.Children.Add(CreateTreeItemRecursive(guiComponent.RootWindow));

            var treeView = new TreeView
            {
                Width = 300,
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
                Text = element.Name,
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
