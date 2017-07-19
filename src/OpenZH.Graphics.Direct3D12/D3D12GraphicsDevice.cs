using SharpDX.Direct3D12;

namespace OpenZH.Graphics.Direct3D12
{
    public sealed class D3D12GraphicsDevice : GraphicsDevice
    {
        public Device Device { get; }

        public override CommandQueue CommandQueue { get; }

        public D3D12GraphicsDevice()
        {
#if DEBUG
            DebugInterface.Get().EnableDebugLayer();
#endif

            Device = AddDisposable(new Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0));

            CommandQueue = new D3D12CommandQueue(Device);
        }

        public override RenderPassDescriptor CreateRenderPassDescriptor()
        {
            return new D3D12RenderPassDescriptor();
        }

        public override ResourceUploadBatch CreateResourceUploadBatch()
        {
            return new D3D12ResourceUploadBatch(Device);
        }

        public override Texture CreateTexture2D(PixelFormat pixelFormat, int width, int height, int numMipmapLevels)
        {
            var resourceDescription = ResourceDescription.Texture2D(
                pixelFormat.ToDxgiFormat(),
                width,
                height,
                mipLevels: (short) numMipmapLevels);

            return new D3D12Texture(this, resourceDescription);
        }
    }
}
