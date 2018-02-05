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
            
            CreateGame = h =>
            {
                var game = createGame(h);

                game.Scene = scene;

                var aptWindow = game.ContentManager.Load<AptWindow>(entry.FilePath);
                scene.Scene2D.AptWindowManager.PushWindow(aptWindow);

                return game;
            };
        }
    }
}
