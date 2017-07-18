using SharpDX.Direct3D12;

namespace OpenZH.Graphics.Direct3D12
{
    public sealed class D3D12RenderTarget : RenderTarget
    {
        public Resource Texture { get; }
        public CpuDescriptorHandle CpuDescriptorHandle { get; }

        public D3D12RenderTarget(Resource texture, CpuDescriptorHandle cpuDescriptorHandle)
        {
            Texture = texture;
            CpuDescriptorHandle = cpuDescriptorHandle;
        }
    }
}
