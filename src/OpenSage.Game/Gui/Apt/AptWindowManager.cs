using System.Collections.Generic;
using System.Linq;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public sealed class AptWindowManager
    {
        private readonly Game _game;
        private AptInputMessageHandler _inputHandler;
        private AptWindow _newWindow;

        internal Stack<AptWindow> WindowStack { get; }
        public int OpenWindowCount => WindowStack.Count;


        public AptWindowManager(Game game)
        {
            _game = game;
            _inputHandler = new AptInputMessageHandler(this, _game);

            WindowStack = new Stack<AptWindow>();

            game.InputMessageBuffer.Handlers.Add(_inputHandler);
        }

        public void PushWindow(AptWindow window)
        {
            CreateSizeDependentResources(window, _game.Panel.ClientBounds.Size);

            window.InputHandler = _inputHandler;

            WindowStack.Push(window);
        }

        public void PopWindow()
        {
            var popped = WindowStack.Pop();
            popped.Dispose();
        }

        internal void OnViewportSizeChanged(in Size newSize)
        {
            foreach (var window in WindowStack)
            {
                CreateSizeDependentResources(window, newSize);
            }
        }

        private void CreateSizeDependentResources(AptWindow window, in Size newSize)
        {
            window.Layout(_game.GraphicsDevice, newSize);
        }

        internal void Update(in TimeInterval gameTime)
        {
            if (_newWindow != null)
            {
                WindowStack.Clear();
                WindowStack.Push(_newWindow);
                _newWindow = null;
            }

            foreach (var window in WindowStack)
            {
                window.Update(gameTime, _game.GraphicsDevice);
            }
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            // TODO: Try to avoid using LINQ here.
            foreach (var window in WindowStack.Reverse())
            {
                window.Render(drawingContext);
            }
        }

        internal bool HandleInput(Point2D mousePos, bool mouseDown)
        {
            foreach (var window in WindowStack)
            {
                if (window.HandleInput(mousePos, mouseDown))
                {
                    return true;
                }
            }

            return false;
        }

        public void QueryTransition(AptWindow newWindow)
        {
            _newWindow = newWindow;
        }
    }
}
