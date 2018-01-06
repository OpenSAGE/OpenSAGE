using OpenSage.Gui.Wnd;

namespace OpenSage
{
    public sealed class Scene2D
    {
        public WndWindowManager WndWindowManager { get; }

        public Scene2D(Game game)
        {
            WndWindowManager = new WndWindowManager(game);
        }

        internal void Update(GameTime gameTime)
        {
            WndWindowManager.Update(gameTime);
        }
    }
}
