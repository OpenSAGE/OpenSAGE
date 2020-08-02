using OpenSage.Gui.ControlBar;
using OpenSage.Logic;

namespace OpenSage.Mods.Bfme2
{
    class AptControlBar : IControlBar
    {
        Game _game;

        public AptControlBar(Game game)
        {
            _game = game;
        }

        public void AddToScene(Scene2D scene2D)
        {
            var aptWindow = _game.LoadAptWindow("Palantir.apt");

            _game.Scene2D.AptWindowManager.PushWindow(aptWindow);
        }

        public void Update(Player player)
        {
        }
    }

    class AptControlBarSource : IControlBarSource
    {
        public IControlBar Create(string side, Game game)
        {
            return new AptControlBar(game);
        }
    }
}
