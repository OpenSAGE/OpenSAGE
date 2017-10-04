using System;
using Caliburn.Micro;
using LLGfx.Hosting;
using OpenSage.DataViewer.Framework.Services;
using OpenSage.DataViewer.ViewModels;

namespace OpenSage.DataViewer.Framework.Controls
{
    public sealed class GameControl : GraphicsDeviceControl
    {
        private readonly Game _game;

        public GameControl()
        {
            GraphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;

            _game = IoC.Get<GameService>().Game;

            GraphicsInitialize += OnGraphicsInitialize;
            GraphicsDraw += OnGraphicsDraw;
            GraphicsUninitialized += OnGraphicsUninitialized;
        }

        private IGameViewModel TypedDataContext => (IGameViewModel) DataContext;

        private void OnGraphicsInitialize(object sender, GraphicsEventArgs e)
        {
            _game.Input.InputProvider = new HwndHostInputProvider(this);
            _game.SetSwapChain(SwapChain);

            TypedDataContext.LoadScene(_game);

            _game.ResetElapsedTime();
        }

        private void OnGraphicsDraw(object sender, GraphicsEventArgs e)
        {
            _game.Tick();
        }

        private void OnGraphicsUninitialized(object sender, EventArgs e)
        {
            _game.Scene = null;
            _game.ContentManager.Unload();

            _game.SetSwapChain(null);
            _game.Input.InputProvider = null;

            if (Bootstrapper.Exiting)
            {
                IoC.Get<GameService>().Dispose();
                IoC.Get<GraphicsDeviceManager>().Dispose();
            }
        }
    }
}
