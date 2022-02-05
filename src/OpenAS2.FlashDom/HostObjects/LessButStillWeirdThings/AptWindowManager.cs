using System.Collections.Generic;
using System.Linq;
using OpenSage.Mathematics;

namespace OpenAS2.HostObjects
{
    public sealed class AptWindowManager
    {
        private AptInputMessageHandler _inputHandler;
        private AptWindow _newWindow;
        private AptWindow _pushWindow;

        internal Stack<AptWindow> WindowStack { get; }
        public int OpenWindowCount => WindowStack.Count;
        public Game Game { get; }


        public AptWindowManager(Game game)
        {
            Game = game;
            _inputHandler = new AptInputMessageHandler(this, Game);

            WindowStack = new Stack<AptWindow>();

            game.InputMessageBuffer.Handlers.Add(_inputHandler);
        }

        public void PushWindow(AptWindow window)
        {
            CreateSizeDependentResources(window, Game.Panel.ClientBounds.Size);

            window.InputHandler = _inputHandler;
            window.Manager = this;

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
            window.Layout(Game.GraphicsDevice, newSize);
        }

        internal void Update(in TimeInterval gameTime)
        {
            if (_pushWindow != null)
            {
                PushWindow(_pushWindow); 
                _pushWindow = null;
            }

            if (_newWindow != null)
            {
                WindowStack.Clear();
                PushWindow(_newWindow);
                _newWindow = null;
            }

            foreach (var window in WindowStack)
            {
                window.Update(gameTime, Game.GraphicsDevice);
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

        public void QueryPush(AptWindow pushWindow)
        {
            _pushWindow = pushWindow;
        }
    }
}
