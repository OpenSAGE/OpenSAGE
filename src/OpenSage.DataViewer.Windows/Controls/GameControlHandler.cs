using System.Windows.Forms.Integration;
using Eto.Drawing;
using Eto.Wpf.Forms;
using OpenSage.DataViewer.Controls;

namespace OpenSage.DataViewer.Windows.Controls
{
    internal sealed class GameControlHandler : WpfFrameworkElement<WindowsFormsHost, GameControl, Eto.Forms.Control.ICallback>, GameControl.IGameControl
    {
        private readonly GameView _gameView;

        public GameControlHandler()
        {
            _gameView = new GameView();

            Control = new WindowsFormsHost
            {
                Child = _gameView
            };
        }

        public override Color BackgroundColor { get => Colors.Transparent; set { } }

        public Game Game
        {
            get => _gameView.Game;
            set => _gameView.Game = value;
        }
    }
}
