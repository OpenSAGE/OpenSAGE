using OpenSage.Data;
using OpenSage.DataViewer.Controls;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class MapView : GameControl
    {
        public MapView(FileSystemEntry entry, Game game)
        {
            Game = game;

            game.Scene = game.ContentManager.Load<Scene>(entry.FilePath);
        }
    }
}
