using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class AniView : Panel
    {
        private Cursor _cursor;

        public AniView(FileSystemEntry entry, Func<IntPtr, Game> createGame)
        {
            Content = new TableLayout(
                new Panel
                {
                    BackgroundColor = Colors.White,
                    Padding = 20,
                    Content = new Label
                    {
                        Text = "Move mouse pointer into the area below to see the cursor.",
                        TextAlignment = TextAlignment.Center
                    }
                },
                new GameControl
                {
                    CreateGame = h =>
                    {
                        var game = createGame(h);
                        _cursor = new Cursor(Path.Combine(game.ContentManager.FileSystem.RootDirectory, entry.FilePath));
                        game.SetCursor(_cursor);
                        return game;
                    },
                });
        }

        protected override void OnUnLoad(EventArgs e)
        {
            _cursor?.Dispose();

            base.OnUnLoad(e);
        }
    }
}
