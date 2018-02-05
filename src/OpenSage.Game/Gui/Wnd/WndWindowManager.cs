using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics;
using OpenSage.Gui.Wnd.Transitions;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowManager
    {
        private readonly Game _game;

        private readonly Stack<WndTopLevelWindow> _windowStack;
        public int OpenWindowCount => _windowStack.Count;

        public WindowTransitionManager TransitionManager { get; }

        public WndWindowManager(Game game)
        {
            _game = game;
            _windowStack = new Stack<WndTopLevelWindow>();

            game.Input.MessageBuffer.Handlers.Insert(0, new WndInputMessageHandler(this));

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

        public WndTopLevelWindow PushWindow(WndTopLevelWindow window)
        {
            CreateSizeDependentResources(window);

            _windowStack.Push(window);

            window.LayoutInit?.Invoke(window);

            return window;
        }

        public WndTopLevelWindow PushWindow(string wndFileName)
        {
            var wndFilePath = Path.Combine("Window", wndFileName);
            var window = _game.ContentManager.Load<WndTopLevelWindow>(wndFilePath);

            if (window == null)
            {
                throw new Exception($"Window file {wndFilePath} was not found.");
            }

            return PushWindow(window);
        }

        internal void OnViewportSizeChanged()
        {
            foreach (var window in _windowStack)
            {
                CreateSizeDependentResources(window);
            }
        }

        private void CreateSizeDependentResources(WndTopLevelWindow window)
        {
            var viewport = _game.Scene.Camera.Viewport;
            var size = new Size((int) viewport.Width, (int) viewport.Height);

            window.Root.DoActionRecursive(
            x =>
            {
                x.CreateSizeDependentResources(_game.ContentManager, size);
                return true;
            });
        }

        public void PopWindow()
        {
            _windowStack.Pop();
        }

        public WndWindow FindWindow(in Vector2 mousePosition)
        {
            if (_windowStack.Count == 0)
            {
                return null;
            }

            var window = _windowStack.Peek();

            return window.FindWindow(mousePosition);
        }

        internal void Update(GameTime gameTime)
        {
            foreach (var window in _windowStack)
            {
                window.LayoutUpdate?.Invoke(window);
            }

            TransitionManager.Update(gameTime);

            foreach (var window in _windowStack)
            {
                window.Root.DoActionRecursive(x =>
                {
                    if (x.IsInvalidated)
                    {
                        x.DrawCallback.Invoke(x, _game);
                        x.IsInvalidated = false;
                    }
                    return true;
                });
            }
        }

        internal void Render(SpriteBatch spriteBatch)
        {
            // TODO: Try to avoid using LINQ here.
            foreach (var window in _windowStack.Reverse())
            {
                window.Root.DoActionRecursive(x =>
                {
                    if (!x.Visible)
                    {
                        return false;
                    }

                    x.Render(spriteBatch);

                    return true;
                });
            }
        }
    }
}
