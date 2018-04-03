using System.Collections.Generic;
using System.Linq;
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
            CreateSizeDependentResources(window, _game.Panel.ClientBounds.Size);

            _windowStack.Push(window);
        }

        internal void OnViewportSizeChanged(in Size newSize)
        {
            foreach (var window in _windowStack)
            {
                CreateSizeDependentResources(window, newSize);
            }
        }

        private void CreateSizeDependentResources(AptWindow window, in Size newSize)
        {
            window.Layout(_game.GraphicsDevice, newSize);
        }

        internal void Update(GameTime gameTime)
        {
            foreach (var window in _windowStack)
            {
                window.Update(gameTime, _game.GraphicsDevice);
            }
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            // TODO: Try to avoid using LINQ here.
            foreach (var window in _windowStack.Reverse())
            {
                window.Render(drawingContext);
            }
        }
    }
}
