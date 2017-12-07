using SharpDX.Direct3D11;

namespace LL.Graphics3D
{
    partial class RenderTarget
    {
        internal RenderTargetView DeviceRenderTargetView { get; private set; }

        internal override string PlatformGetDebugName() => DeviceRenderTargetView.DebugName;
        internal override void PlatformSetDebugName(string value) => DeviceRenderTargetView.DebugName = value;

        internal RenderTarget(GraphicsDevice graphicsDevice, RenderTargetView renderTargetView)
            : base(graphicsDevice)
        {
            DeviceRenderTargetView = renderTargetView;
        }

        private void PlatformConstruct(GraphicsDevice graphicsDevice, Texture texture)
        {
            DeviceRenderTargetView = AddDisposable(new RenderTargetView(
                graphicsDevice.Device,
                texture.DeviceResource));
        }
    }
}
