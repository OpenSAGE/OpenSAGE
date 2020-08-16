using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.Wnd;
using OpenSage.Input.Cursors;
using OpenSage.Logic;

namespace OpenSage
{
    public sealed class Scene2D
    {
        public WndWindowManager WndWindowManager { get; }
        public AptWindowManager AptWindowManager { get; }
        public IControlBar ControlBar { get; set; }

        private CursorManager _cursorManager;

        public Scene2D(Game game)
        {
            WndWindowManager = new WndWindowManager(game);
            AptWindowManager = new AptWindowManager(game);
            _cursorManager = game.Cursors;
        }

        internal void LocalLogicTick(in TimeInterval gameTime, Player localPlayer)
        {
            ControlBar?.Update(localPlayer);

            WndWindowManager.Update(gameTime);
            AptWindowManager.Update(gameTime);
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            WndWindowManager.Render(drawingContext);
            AptWindowManager.Render(drawingContext);
            // TODO: the cursor should be drawn somewhere where it's rendered
            // on the entire window, not just the game view
            _cursorManager.Render(drawingContext);
        }
    }
}
