using System;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using Eto.Drawing;
using Eto.Wpf.Forms;
using LL.Graphics3D.Hosting;
using LL.Input;
using OpenSage.DataViewer.Controls;
using OpenSage.Input.Providers;

namespace OpenSage.DataViewer.Windows.Controls
{
    internal sealed class GameControlHandler : WpfFrameworkElement<WindowsFormsHost, GameControl, Eto.Forms.Control.ICallback>, GameControl.IGameControl
    {
        public GameControlHandler()
        {
            var graphicsView = new InputView
            {
                GraphicsDevice = DataViewerApplication.Instance.GraphicsDevice
            };

            graphicsView.GraphicsInitialize += (sender, e) =>
            {
                Game.Input.InputProvider = new InputProvider(graphicsView);
                Game.SetSwapChain(graphicsView.SwapChain);

                Widget.OnGraphicsInitialized();
            };

            graphicsView.GraphicsUninitialized += (sender, e) =>
            {
                Widget.OnGraphicsUninitialized();

                Game.SetSwapChain(null);
                Game.Input.InputProvider = null;
            };

            graphicsView.GraphicsDraw += (sender, e) =>
            {
                Widget.OnGraphicsDraw();
            };

            graphicsView.GraphicsResized += (sender, e) =>
            {
                Game.SetSwapChain(graphicsView.SwapChain);
            };

            var windowsFormsHost = new WindowsFormsHost
            {
                Child = graphicsView
            };

            Control = windowsFormsHost;
        }

        public override Color BackgroundColor { get => Colors.Transparent; set { } }

        public Game Game { get; set; }

        public override void OnLoadComplete(EventArgs e)
        {
            Control.Focus();

            base.OnLoadComplete(e);
        }
    }
}
