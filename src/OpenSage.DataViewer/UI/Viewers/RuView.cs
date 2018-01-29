using System;
using System.IO;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;
using OpenSage.Gui.Apt;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class RuView : Splitter
    {
        public RuView(FileSystemEntry entry, Func<IntPtr, Game> createGame)
        {
            var scene = new Scene();
            var entity = new Entity();

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
                CreateGame = h =>
                {
                    var game = createGame(h);

                    var guiComponent = game.ContentManager.Load<ShapeComponent>(entry.FilePath);
                    entity.Components.Add(guiComponent);
                    scene.Entities.Add(entity);

                    game.Scene = scene;

                    return game;
                }
            };
        }
    }
}
