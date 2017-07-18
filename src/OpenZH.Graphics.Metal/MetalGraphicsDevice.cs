using Metal;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalGraphicsDevice : GraphicsDevice
    {
        public IMTLDevice Device { get; }

        public override CommandQueue CommandQueue { get; }

        public MetalGraphicsDevice()
        {
            Device = MTLDevice.SystemDefault;

            CommandQueue = new MetalCommandQueue(Device);
        }

        public override RenderPassDescriptor CreateRenderPassDescriptor()
        {
            return new MetalRenderPassDescriptor();
        }
    }
}