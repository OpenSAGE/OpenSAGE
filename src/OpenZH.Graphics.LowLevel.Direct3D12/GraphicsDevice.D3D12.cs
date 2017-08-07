using System;
using SharpDX.Direct3D12;
using OpenZH.Graphics.LowLevel.Util;

namespace OpenZH.Graphics.LowLevel
{
    partial class GraphicsDevice
    {
        private Fence _numCompletedFramesFence;
        private long _numCompletedFrames;
        private long _currentFrame;

        internal Device Device { get; private set; }

        internal DescriptorTablePool DescriptorHeapCbvUavSrv { get; private set; }

        internal DynamicUploadHeap DynamicUploadHeap { get; private set; }

        private void PlatformConstruct()
        {
#if DEBUG
            DebugInterface.Get().EnableDebugLayer();
#endif

            Device = AddDisposable(new Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0));

            DescriptorHeapCbvUavSrv = AddDisposable(new DescriptorTablePool(
                Device,
                DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
                1_000_000)); // Maximum descriptor count for Tier 1 hardware

            _numCompletedFramesFence = AddDisposable(Device.CreateFence(0, FenceFlags.None));

            const uint initialDynamicUploadHeapSize = 1024 * 1024; // TODO
            DynamicUploadHeap = AddDisposable(new DynamicUploadHeap(Device, initialDynamicUploadHeapSize));
        }

        internal void FinishFrame()
        {
            _numCompletedFrames = Math.Max(_numCompletedFrames, _numCompletedFramesFence.CompletedValue);

            DynamicUploadHeap.FinishFrame(_currentFrame, _numCompletedFrames);

            _currentFrame += 1;

            CommandQueue.DeviceCommandQueue.Signal(_numCompletedFramesFence, _currentFrame);
        }
    }
}
