using SharpDX.DXGI;

namespace OpenZH.DataViewer.UWP.Rendering
{
    using System;
    using SharpDX.Direct3D12;

    public class D3D12RenderApi : IDisposable
    {
        private readonly Device _device;
        private readonly CommandQueue _commandQueue;

        public D3D12RenderApi()
        {
#if DEBUG
            DebugInterface.Get().EnableDebugLayer();
#endif

            _device = new Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0);

            _commandQueue = _device.CreateCommandQueue(new CommandQueueDescription(CommandListType.Direct));
        }

        public void Dispose()
        {

        }
    }
}
