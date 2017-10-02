using System;
using System.Windows;
using Caliburn.Micro;
using LLGfx.Hosting;
using OpenSage.DataViewer.Framework.Services;

namespace OpenSage.DataViewer.Framework.Controls
{
    public sealed class GameControl : GraphicsDeviceControl
    {
        public static readonly DependencyProperty GameProperty = DependencyProperty.Register(
            nameof(Game), typeof(Game), typeof(GameControl),
            new PropertyMetadata(null, OnGameChanged));

        private static void OnGameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GameControl) d).GraphicsDevice = ((Game) e.NewValue).GraphicsDevice;
        }

        public Game Game
        {
            get { return (Game) GetValue(GameProperty); }
            set { SetValue(GameProperty, value); }
        }

        public GameControl()
        {
            GraphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;

            RedrawsOnTimer = true;
        }

        protected override void RaiseGraphicsInitialize(GraphicsEventArgs args)
        {
            base.RaiseGraphicsInitialize(args);

            Game.Input.InputProvider = new HwndHostInputProvider(this);
            Game.SetSwapChain(SwapChain);
        }

        protected override void Draw()
        {
            Game.Tick();
        }
    }
}
