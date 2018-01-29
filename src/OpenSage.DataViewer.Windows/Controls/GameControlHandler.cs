using System;
using System.Windows.Forms.Integration;
using Eto.Drawing;
using Eto.Wpf.Forms;
using OpenSage.DataViewer.Controls;

namespace OpenSage.DataViewer.Windows.Controls
{
    internal sealed class GameControlHandler : WpfFrameworkElement<WindowsFormsHost, GameControl, Eto.Forms.Control.ICallback>, GameControl.IGameControl
    {
        private Game _game;

        public Func<IntPtr, Game> CreateGame { get; set; }

        public GameControlHandler()
        {
            var control = new System.Windows.Forms.Control
            {
                Width = 100,
                Height = 100
            };

            control.HandleCreated += (sender, e) =>
            {
                _game = CreateGame(control.Handle);
                System.Windows.Media.CompositionTarget.Rendering += OnRendering;
            };

            control.MouseDown += (sender, e) =>
            {
                control.Focus();
            };

            Control = new WindowsFormsHost
            {
                Child = control
            };
        }

        public override void OnUnLoad(EventArgs e)
        {
            System.Windows.Media.CompositionTarget.Rendering -= OnRendering;
            _game = null;

            base.OnUnLoad(e);
        }

        private void OnRendering(object sender, EventArgs e)
        {
            _game?.Tick();
        }

        public override Color BackgroundColor { get => Colors.Transparent; set { } }
    }
}
