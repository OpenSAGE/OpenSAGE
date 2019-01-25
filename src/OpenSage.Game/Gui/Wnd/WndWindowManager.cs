using System;
using System.Collections.Generic;
using System.IO;
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

            switch (game.SageGame)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\WindowTransitions.ini");
                    TransitionManager = new WindowTransitionManager(game.ContentManager.IniDataContext.WindowTransitions);
                    break;

                default: // TODO: Handle other games.
                    TransitionManager = new WindowTransitionManager(new List<Data.Ini.WindowTransition>());
                    break;
            }
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
            var wndFilePath = Path.Combine("Window", wndFileName);
            var window = _game.ContentManager.Load<Window>(wndFilePath, new Content.LoadOptions { CacheAsset = false });

            if (window == null)
            {
                throw new Exception($"Window file {wndFilePath} was not found.");
            }

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

        private void CreateSizeDependentResources(Window window, Size newSize)
        {
            window.Size = newSize;
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
