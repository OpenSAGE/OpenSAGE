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
            CreateGame = h =>
            {
                var game = createGame(h);

                var aptWindow = game.ContentManager.Load<AptWindow>(entry.FilePath);
                game.Scene2D.AptWindowManager.PushWindow(aptWindow);

                return game;
            };
        }
    }
}
