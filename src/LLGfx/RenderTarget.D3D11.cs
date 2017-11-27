using SharpDX.Direct3D11;

namespace LLGfx
{
    partial class RenderTarget
    {
        internal RenderTargetView DeviceRenderTargetView { get; }

        internal RenderTarget(GraphicsDevice graphicsDevice, RenderTargetView renderTargetView)
            : base(graphicsDevice)
        {
            DeviceRenderTargetView = renderTargetView;
        }
    }
}
