using Eto.Forms;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.DataViewer.Controls;
using OpenSage.Gui.Apt;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class RuView : GameControl
    {
        public RuView(FileSystemEntry entry, Game game)
        {
            var scene = new Scene();
            var entity = new Entity();

            var guiComponent = game.ContentManager.Load<ShapeComponent>(entry.FilePath);
            entity.Components.Add(guiComponent);
            scene.Entities.Add(entity);

            game.Scene = scene;
            Game = game;
        }

       
    }
}
