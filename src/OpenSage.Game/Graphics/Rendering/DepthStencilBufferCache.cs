using LL.Graphics3D;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class DepthStencilBufferCache : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;
        private DepthStencilBuffer _depthStencilBuffer;

        public DepthStencilBufferCache(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public DepthStencilBuffer Get(int width, int height)
        {
            if (_depthStencilBuffer != null
                && _depthStencilBuffer.Width == width
                && _depthStencilBuffer.Height == height)
            {
                return _depthStencilBuffer;
            }

            RemoveAndDispose(_depthStencilBuffer);

            _depthStencilBuffer = AddDisposable(new DepthStencilBuffer(
                _graphicsDevice,
                width,
                height,
                1.0f));

            return _depthStencilBuffer;
        }
    }
}
