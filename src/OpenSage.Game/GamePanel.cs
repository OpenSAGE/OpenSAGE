using System;
using OpenSage.Input;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage
{
    public abstract class GamePanel : DisposableBase
    {
        public abstract GraphicsDevice GraphicsDevice { get; }

        public abstract event EventHandler FramebufferChanged;

        public abstract Framebuffer Framebuffer { get; }

        public abstract event EventHandler ClientSizeChanged;

        public abstract Rectangle ClientBounds { get; }

        public event EventHandler<InputMessageEventArgs> InputMessageReceived; // TODO

        public void SetCursor(Cursor cursor)
        {
            // TODO
        }

        public static GamePanel FromGameWindow(GameWindow window) => new GameWindowPanel(window);

        private sealed class GameWindowPanel : GamePanel
        {
            private readonly GameWindow _window;

            public override event EventHandler FramebufferChanged;

            public override GraphicsDevice GraphicsDevice => _window.GraphicsDevice;

            public override Framebuffer Framebuffer => _window.GraphicsDevice.SwapchainFramebuffer;

            public override event EventHandler ClientSizeChanged
            {
                add => _window.ClientSizeChanged += value;
                remove => _window.ClientSizeChanged -= value;
            }

            public override Rectangle ClientBounds => _window.ClientBounds;

            public GameWindowPanel(GameWindow window)
            {
                _window = window;
            }
        }
    }
}
