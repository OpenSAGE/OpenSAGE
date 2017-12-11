using Eto.Drawing;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class AniView : Panel
    {
        public AniView(FileSystemEntry entry)
        {
            Content = new TableLayout(
                new TableRow() { ScaleHeight = true },
                new Label { Text = "Move mouse pointer into this box to see the cursor.", TextAlignment = TextAlignment.Center },
                new AniCursorPreview { AniFile = entry, BackgroundColor = Colors.White, Width = 200, Height = 200 },
                new TableRow() { ScaleHeight = true })
            {
                Padding = new Padding(20)
            };
        }
    }
}
