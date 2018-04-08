using OpenSage.Graphics;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Wnd;

namespace OpenSage
{
    public sealed class Scene2D
    {
        public WndWindowManager WndWindowManager { get; }
        public AptWindowManager AptWindowManager { get; }

        public SelectionGui SelectionGui { get; internal set; }

        public Scene2D(Game game)
        {
            WndWindowManager = new WndWindowManager(game);
            AptWindowManager = new AptWindowManager(game);
            SelectionGui = new SelectionGui();
        }

        internal void Update(GameTime gameTime)
        {
            WndWindowManager.Update(gameTime);
            AptWindowManager.Update(gameTime);
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            WndWindowManager.Render(drawingContext);
            AptWindowManager.Render(drawingContext);
            SelectionGui.Draw(drawingContext);
        }
    }
}
