using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;
using OpenSage.Gui.Apt;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class RuView : Splitter
    {
        public RuView(FileSystemEntry entry, Game game)
        {
            var guiComponent = new ShapeComponent
            {
                Window = game.ContentManager.Load<ShapeWindow>(entry.FilePath)
            };

            var scene = new Scene();

            var entity = new Entity();
            entity.Components.Add(guiComponent);
            scene.Entities.Add(entity);

            game.Scene = scene;
        }

       
    }
}
