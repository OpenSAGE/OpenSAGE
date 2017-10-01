using System.Windows;
using Caliburn.Micro;
using LLGfx.Hosting;
using OpenSage.DataViewer.Framework.Services;
using OpenSage.DataViewer.ViewModels;

namespace OpenSage.DataViewer.Framework.Controls
{
    public sealed class GameControl : GraphicsDeviceControl
    {
        private IGameViewModel TypedDataContext => (IGameViewModel) DataContext;

        public GameControl()
        {
            GraphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;

            RedrawsOnTimer = true;
        }

        protected override void OnUnloaded(object sender, RoutedEventArgs e)
        {
            base.OnUnloaded(sender, e);

            TypedDataContext.Game.SetSwapChain(null);
            TypedDataContext.Game.Input.InputProvider = null;

            TypedDataContext.Game.Scene = null;

            TypedDataContext.Game.ContentManager.Unload();

            TypedDataContext.Dispose();
        }

        protected override void RaiseGraphicsInitialize(GraphicsEventArgs args)
        {
            base.RaiseGraphicsInitialize(args);

            TypedDataContext.Game.Input.InputProvider = new HwndHostInputProvider(this);
            TypedDataContext.Game.SetSwapChain(SwapChain);
        }

        protected override void Draw()
        {
            TypedDataContext.Game.Tick();
        }
    }
}
