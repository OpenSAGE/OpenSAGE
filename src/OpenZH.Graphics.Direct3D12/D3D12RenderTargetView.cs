using SharpDX.Direct3D12;

namespace OpenZH.Graphics.Direct3D12
{
    public sealed class D3D12RenderTargetView : RenderTargetView
    {
        public Resource RenderTarget { get; }
        public CpuDescriptorHandle CpuDescriptorHandle { get; }

        public D3D12RenderTargetView(Resource renderTarget, CpuDescriptorHandle cpuDescriptorHandle)
        {
            RenderTarget = renderTarget;
            CpuDescriptorHandle = cpuDescriptorHandle;
        }
    }
}
