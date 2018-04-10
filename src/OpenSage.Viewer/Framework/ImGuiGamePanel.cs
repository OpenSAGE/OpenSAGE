using System;
using ImGuiNET;
using OpenSage.Input;
using Veldrid;

namespace OpenSage.Viewer.Framework
{
    internal sealed class ImGuiGamePanel : GamePanel
    {
        private readonly GraphicsDevice _graphicsDevice;

        private Texture _gameColorTarget;
        private Texture _gameDepthTarget;
        private Framebuffer _gameFramebuffer;

        private uint _width = uint.MaxValue;
        private uint _height = uint.MaxValue;

        public override GraphicsDevice GraphicsDevice => _graphicsDevice;

        public override event EventHandler FramebufferChanged;

        public override Framebuffer Framebuffer => _gameFramebuffer;

        public override Mathematics.Rectangle ClientBounds => new Mathematics.Rectangle(0, 0, (int) _width, (int) _height);

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

            // TODO: Map mouse position.
            InputMessageReceived?.Invoke(this, e);
        }

        public void EnsureSize(uint width, uint height)
        {
            if (_width == width && _height == height)
            {
                return;
            }

            _width = width;
            _height = height;

            RemoveAndDispose(ref _gameFramebuffer);
            RemoveAndDispose(ref _gameDepthTarget);
            RemoveAndDispose(ref _gameColorTarget);

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
