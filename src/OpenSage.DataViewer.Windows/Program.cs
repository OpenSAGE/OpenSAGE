using System;
using OpenSage.DataViewer.Controls;
using OpenSage.DataViewer.UI;
using OpenSage.DataViewer.Windows.Controls;

namespace OpenSage.DataViewer
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.Wpf.Platform();

            platform.Add<AniCursorPreview.IAniCursorPreview>(() => new AniCursorPreviewHandler());
            platform.Add<GameControl.IGameControl>(() => new GameControlHandler());

            using (var app = new DataViewerApplication(platform))
            using (var mainForm = new MainForm())
            {
                app.Run(mainForm);
            }
        }
    }
}
