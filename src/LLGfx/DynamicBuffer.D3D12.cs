using LLGfx.Util;
using SharpDX;

namespace LLGfx
{
    partial class DynamicBuffer<T>
    {
        private DynamicAllocation _currentAllocation;

        internal override long DeviceCurrentGPUVirtualAddress => _currentAllocation.GpuAddress;

        private void PlatformSetData(T[] data)
        {
            _currentAllocation = GraphicsDevice.DynamicUploadHeap.Allocate(SizeInBytes);
            Utilities.Write(_currentAllocation.CpuAddress, data, 0, data.Length);
        }

        private void PlatformSetData(ref T data)
        {
            _currentAllocation = GraphicsDevice.DynamicUploadHeap.Allocate(SizeInBytes);
            Utilities.Write(_currentAllocation.CpuAddress, ref data);
        }
    }
}
