using System;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;
using OpenSage.Gui.Apt;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class AptView : GameControl
    {
        public AptView(FileSystemEntry entry, Func<IntPtr, Game> createGame)
        {
            var scene = new Scene();
            var entity = new Entity();
            
            CreateGame = h =>
            {
                var game = createGame(h);

                var guiComponent = game.ContentManager.Load<AptComponent>(entry.FilePath);
                entity.Components.Add(guiComponent);
                scene.Entities.Add(entity);

                game.Scene = scene;

                return game;
            };
        }
    }
}
