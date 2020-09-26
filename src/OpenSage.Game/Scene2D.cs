using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.UnitOverlay;
using OpenSage.Gui.Wnd;
using OpenSage.Logic;

namespace OpenSage
{
    public sealed class Scene2D
    {
        public WndWindowManager WndWindowManager { get; }
        public AptWindowManager AptWindowManager { get; }
        public IControlBar ControlBar { get; set; }
        public IUnitOverlay UnitOverlay { get; set; }

        public Scene2D(Game game)
        {
            WndWindowManager = new WndWindowManager(game);
            AptWindowManager = new AptWindowManager(game);
        }

        internal void LocalLogicTick(in TimeInterval gameTime, Player localPlayer)
        {
            ControlBar?.Update(localPlayer);
            UnitOverlay?.Update(localPlayer);

            WndWindowManager.Update(gameTime);
            AptWindowManager.Update(gameTime);
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            UnitOverlay?.Render(drawingContext);

            WndWindowManager.Render(drawingContext);
            AptWindowManager.Render(drawingContext);
        }
    }
}
