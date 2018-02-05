using System.Collections.Generic;
using System.Linq;
using OpenSage.Graphics;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public sealed class AptWindowManager
    {
        private readonly Game _game;

        private readonly Stack<AptWindow> _windowStack;

        public AptWindowManager(Game game)
        {
            _game = game;

            _windowStack = new Stack<AptWindow>();
        }

        public void PushWindow(AptWindow window)
        {
            CreateSizeDependentResources(window);

            _windowStack.Push(window);
        }

        internal void OnViewportSizeChanged()
        {
            foreach (var window in _windowStack)
            {
                CreateSizeDependentResources(window);
            }
        }

        private void CreateSizeDependentResources(AptWindow window)
        {
            var viewport = _game.Scene.Camera.Viewport;
            var size = new Size((int) viewport.Width, (int) viewport.Height);

            window.Layout(_game.GraphicsDevice, size);
        }

        internal void Update(GameTime gameTime)
        {
            foreach (var window in _windowStack)
            {
                window.Update(gameTime, _game.GraphicsDevice);
            }
        }

        internal void Render(SpriteBatch spriteBatch)
        {
            // TODO: Try to avoid using LINQ here.
            foreach (var window in _windowStack.Reverse())
            {
                window.Render(spriteBatch);
            }
        }
    }
}
