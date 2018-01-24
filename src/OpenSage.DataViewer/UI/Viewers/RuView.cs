using System.IO;
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
            var scene = new Scene();
            var entity = new Entity();

            var guiComponent = game.ContentManager.Load<ShapeComponent>(entry.FilePath);
            entity.Components.Add(guiComponent);
            scene.Entities.Add(entity);

            game.Scene = scene;

            string ruText;
            using (var fileStream = entry.Open())
            using (var streamReader = new StreamReader(fileStream))
                ruText = streamReader.ReadToEnd();

            Panel1 = new TextBox
            {
                ReadOnly = true,
                Width = 250,
                Text = ruText
            };

            Panel2 = new GameControl
            {
                Game = game
            };
        }
    }
}
