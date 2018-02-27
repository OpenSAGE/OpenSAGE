using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Graphics;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Transitions;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowManager
    {
        private readonly Game _game;

        private readonly Stack<Window> _windowStack;
        public int OpenWindowCount => _windowStack.Count;

        public WindowTransitionManager TransitionManager { get; }

        public WndWindowManager(Game game)
        {
            _game = game;
            _windowStack = new Stack<Window>();

            game.InputMessageBuffer.Handlers.Insert(0, new WndInputMessageHandler(this, _game));

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
            window.Size = _game.Window.ClientBounds.Size;

            _windowStack.Push(window);

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

            if (_windowStack.Count > 0)
            {
                PopWindow();
            }

            return PushWindow(wndFileName);
        }

        internal void OnViewportSizeChanged(in Size newSize)
        {
            foreach (var window in _windowStack)
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
            var popped = _windowStack.Pop();
            popped.Dispose();
        }

        public Control GetControlAtPoint(in Point2D mousePosition)
        {
            if (_windowStack.Count == 0)
            {
                return null;
            }

            var window = _windowStack.Peek();

            return window.GetSelfOrDescendantAtPoint(mousePosition);
        }

        public Control[] GetControlsAtPoint(in Point2D mousePosition)
        {
            if (_windowStack.Count == 0)
            {
                return new Control[0];
            }

            var window = _windowStack.Peek();

            return window.GetSelfOrDescendantsAtPoint(mousePosition);
        }

        internal void Update(GameTime gameTime)
        {
            foreach (var window in _windowStack)
            {
                window.LayoutUpdate?.Invoke(window, _game);
            }

            TransitionManager.Update(gameTime);

            foreach (var window in _windowStack)
            {
                window.UpdateTexture();
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
