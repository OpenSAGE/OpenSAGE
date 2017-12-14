using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;
using OpenSage.LowLevel;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class AniView : Panel
    {
        private readonly Game _game;
        private readonly HostCursor _cursor;

        public AniView(FileSystemEntry entry, Game game)
        {
            _game = game;

            _cursor = new HostCursor(Path.Combine(game.ContentManager.FileSystem.RootDirectory, entry.FilePath));

            game.SetCursor(_cursor);

            Content = new TableLayout(
                new Panel { BackgroundColor = Colors.White, Padding = 20, Content = new Label { Text = "Move mouse pointer into the area below to see the cursor.", TextAlignment = TextAlignment.Center } },
                new GameControl { Game = game });
        }

        protected override void OnUnLoad(EventArgs e)
        {
            _game.SetCursor("Arrow");
            _cursor.Dispose();

            base.OnUnLoad(e);
        }
    }
}
