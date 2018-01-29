using System;
using Eto;
using Eto.Forms;

namespace OpenSage.DataViewer.Controls
{
    [Handler(typeof(IGameControl))]
    public class GameControl : Control
    {
        private new IGameControl Handler => (IGameControl) base.Handler;

        public Func<IntPtr, Game> CreateGame
        {
            get { return Handler.CreateGame; }
            set { Handler.CreateGame = value; }
        }

        public interface IGameControl : IHandler
        {
            Func<IntPtr, Game> CreateGame { get; set; }
        }
    }
}
