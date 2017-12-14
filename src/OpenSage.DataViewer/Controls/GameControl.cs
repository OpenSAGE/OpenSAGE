using Eto;
using Eto.Forms;

namespace OpenSage.DataViewer.Controls
{
    [Handler(typeof(IGameControl))]
    public class GameControl : Control
    {
        private new IGameControl Handler => (IGameControl) base.Handler;

        public Game Game
        {
            get { return Handler.Game; }
            set { Handler.Game = value; }
        }

        public interface IGameControl : IHandler
        {
            Game Game { get; set; }
        }
    }
}
