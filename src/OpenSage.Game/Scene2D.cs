using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Wnd;
using OpenSage.Logic;

namespace OpenSage
{
    public sealed class Scene2D
    {
        public WndWindowManager WndWindowManager { get; }
        public AptWindowManager AptWindowManager { get; }
        public IControlBar ControlBar { get; set; }

        public Scene2D(Game game)
        {
            WndWindowManager = new WndWindowManager(game);
            AptWindowManager = new AptWindowManager(game);
        }

        internal void LocalLogicTick(in GameTime gameTime, Player localPlayer)
        {
            ControlBar?.Update(localPlayer);

            WndWindowManager.Update(gameTime);
            AptWindowManager.Update(gameTime);
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            WndWindowManager.Render(drawingContext);
            AptWindowManager.Render(drawingContext);
        }
    }
}
