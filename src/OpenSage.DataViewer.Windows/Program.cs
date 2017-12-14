using System;
using System.Windows.Media;
using Eto.Wpf.Forms.Controls;
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

            platform.Add<GameControl.IGameControl>(() => new GameControlHandler());

            Eto.Style.Add<ImageViewHandler>("nearest-neighbor", handler =>
            {
                RenderOptions.SetBitmapScalingMode(handler.Control, BitmapScalingMode.NearestNeighbor);
            });

            using (var app = new DataViewerApplication(platform))
            using (var mainForm = new MainForm())
            {
                app.Run(mainForm);
            }
        }
    }
}
