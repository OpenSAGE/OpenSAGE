using System;
using OpenSage.Input;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Viewer.Framework
{
    internal sealed class ImGuiGamePanel : GamePanel
    {
        private readonly GraphicsDevice _graphicsDevice;

        private Texture _gameColorTarget;
        private Texture _gameDepthTarget;
        private Framebuffer _gameFramebuffer;

        private Rectangle _frame;

        public override GraphicsDevice GraphicsDevice => _graphicsDevice;

        public override event EventHandler FramebufferChanged;

        public override Framebuffer Framebuffer => _gameFramebuffer;

        public override Rectangle ClientBounds => new Rectangle(0, 0, _frame.Width, _frame.Height);

        public override event EventHandler ClientSizeChanged;

        public override event EventHandler<InputMessageEventArgs> InputMessageReceived;

        public bool IsGameViewActive { get; set; }

        public ImGuiGamePanel(GameWindow gameWindow)
        {
            _graphicsDevice = gameWindow.GraphicsDevice;

            gameWindow.InputMessageReceived += OnInputMessageReceived;

            AddDisposeAction(() => gameWindow.InputMessageReceived -= OnInputMessageReceived);
        }

        private void OnInputMessageReceived(object sender, InputMessageEventArgs e)
        {
            if (!IsGameViewActive)
            {
                return;
            }

            Point2D getPositionInPanel()
            {
                var pos = e.Message.Value.MousePosition;
                pos.X -= _frame.X;
                pos.Y -= _frame.Y;
                return pos;
            }

            switch (e.Message.MessageType)
            {
                case InputMessageType.MouseLeftButtonDown:
                case InputMessageType.MouseLeftButtonUp:
                case InputMessageType.MouseMiddleButtonDown:
                case InputMessageType.MouseMiddleButtonUp:
                case InputMessageType.MouseRightButtonDown:
                case InputMessageType.MouseRightButtonUp:
                    e = new InputMessageEventArgs(InputMessage.CreateMouseButton(
                        e.Message.MessageType,
                        getPositionInPanel()));
                    break;

                case InputMessageType.MouseMove:
                    e = new InputMessageEventArgs(InputMessage.CreateMouseMove(
                        getPositionInPanel()));
                    break;
            }

            InputMessageReceived?.Invoke(this, e);
        }

        public void EnsureFrame(Rectangle frame)
        {
            if (frame == _frame)
            {
                return;
            }

            _frame = frame;

            RemoveAndDispose(ref _gameFramebuffer);
            RemoveAndDispose(ref _gameDepthTarget);
            RemoveAndDispose(ref _gameColorTarget);

            var width = (uint) _frame.Width;
            var height = (uint) _frame.Height;

            _gameColorTarget = AddDisposable(_graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    width,
                    height,
                    1,
                    1,
                    PixelFormat.B8_G8_R8_A8_UNorm,
                    TextureUsage.RenderTarget | TextureUsage.Sampled)));

            _gameDepthTarget = AddDisposable(_graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    width,
                    height,
                    1,
                    1,
                    PixelFormat.D32_Float_S8_UInt,
                    TextureUsage.DepthStencil)));

            _gameFramebuffer = AddDisposable(_graphicsDevice.ResourceFactory.CreateFramebuffer(
                new FramebufferDescription(_gameDepthTarget, _gameColorTarget)));

            ClientSizeChanged?.Invoke(this, EventArgs.Empty);
            FramebufferChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
