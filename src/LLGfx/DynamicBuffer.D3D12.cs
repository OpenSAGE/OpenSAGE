using LLGfx.Util;
using SharpDX;

namespace LLGfx
{
    partial class DynamicBuffer
    {
        private DynamicAllocation _currentAllocation;

        internal override long DeviceCurrentGPUVirtualAddress => _currentAllocation.GpuAddress;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, uint sizeInBytes) { }

        private void PlatformSetData<T>(ref T data)
            where T : struct
        {
            _currentAllocation = GraphicsDevice.DynamicUploadHeap.Allocate(SizeInBytes);
            Utilities.Write(_currentAllocation.CpuAddress, ref data);
        }
    }
}
