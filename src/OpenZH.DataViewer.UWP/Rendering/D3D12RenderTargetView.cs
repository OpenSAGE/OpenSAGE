using OpenZH.DataViewer.Rendering;
using SharpDX.Direct3D12;

namespace OpenZH.DataViewer.UWP.Rendering
{
    public sealed class D3D12RenderTargetView : RenderTargetView
    {
        public CpuDescriptorHandle CpuDescriptorHandle { get; }

        public D3D12RenderTargetView(CpuDescriptorHandle cpuDescriptorHandle)
        {
            CpuDescriptorHandle = cpuDescriptorHandle;
        }
    }
}
