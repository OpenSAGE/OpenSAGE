using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Transitions;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowManager
    {
        private readonly Game _game;

        public int OpenWindowCount => WindowStack.Count;

        internal Stack<Window> WindowStack { get; }

        public WindowTransitionManager TransitionManager { get; }

        public WndWindowManager(Game game)
        {
            _game = game;
            WindowStack = new Stack<Window>();

            game.InputMessageBuffer.Handlers.Add(new WndInputMessageHandler(this, _game));

            TransitionManager = new WindowTransitionManager(game.AssetStore.WindowTransitions);
        }

        public Window PushWindow(Window window)
        {
            window.Size = _game.Panel.ClientBounds.Size;

            WindowStack.Push(window);

            window.LayoutInit?.Invoke(window, _game);

            return window;
        }

        public Window PushWindow(string wndFileName)
        {
            var window = _game.LoadWindow(wndFileName);
            return PushWindow(window);
        }

        public Window SetWindow(string wndFileName)
        {
            // TODO: Handle transitions between windows.

            while (WindowStack.Count > 0)
            {
                PopWindow();
            }

            return PushWindow(wndFileName);
        }

        internal void OnViewportSizeChanged(in Size newSize)
        {
            foreach (var window in WindowStack)
            {
                window.Size = newSize;
            }
        }

        public void PopWindow()
        {
            var popped = WindowStack.Pop();
            popped.Dispose();
        }

        public Control GetControlAtPoint(in Point2D mousePosition)
        {
            if (WindowStack.Count == 0)
            {
                return null;
            }

            var window = WindowStack.Peek();

            return window.GetSelfOrDescendantAtPoint(mousePosition);
        }

        public Control[] GetControlsAtPoint(in Point2D mousePosition)
        {
            if (WindowStack.Count == 0)
            {
                return new Control[0];
            }

            var window = WindowStack.Peek();

            return window.GetSelfOrDescendantsAtPoint(mousePosition);
        }

        internal void Update(in TimeInterval gameTime)
        {
            foreach (var window in WindowStack)
            {
                window.LayoutUpdate?.Invoke(window, _game);
            }

            TransitionManager.Update(gameTime);

            foreach (var window in WindowStack)
            {
                window.Update();
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
    }
}
