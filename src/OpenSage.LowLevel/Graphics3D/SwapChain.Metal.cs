using Metal;
using MetalKit;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class SwapChain
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly MTKView _metalView;

        private int PlatformBackBufferWidth => (int) _metalView.DrawableSize.Width;
        private int PlatformBackBufferHeight => (int) _metalView.DrawableSize.Height;

        internal IMTLDrawable CurrentDrawable => _metalView.CurrentDrawable;

        public SwapChain(
            GraphicsDevice graphicsDevice,
            MTKView metalView)
        {
            _graphicsDevice = graphicsDevice;
            _metalView = metalView;
        }

        private RenderTarget PlatformGetNextRenderTarget()
        {
            return new RenderTarget(
                _graphicsDevice,
                _metalView.CurrentDrawable.Texture);
        }
    }
}
