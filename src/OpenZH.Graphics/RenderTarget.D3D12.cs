using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    public sealed partial class RenderTarget
    {
        internal Resource Texture { get; }

        internal CpuDescriptorHandle CpuDescriptorHandle { get; }

        internal RenderTarget(GraphicsDevice graphicsDevice, Resource texture, CpuDescriptorHandle cpuDescriptorHandle)
            : base(graphicsDevice)
        {
            Texture = texture;
            CpuDescriptorHandle = cpuDescriptorHandle;
        }
    }
}
