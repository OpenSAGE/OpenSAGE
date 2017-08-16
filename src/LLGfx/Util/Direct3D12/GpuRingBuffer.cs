using System;
using SharpDX.Direct3D12;

namespace LLGfx.Util
{
    internal sealed class GpuRingBuffer : RingBuffer, IDisposable
    {
        private readonly Resource _buffer;
        private long _gpuVirtualAddress;
        private IntPtr _cpuVirtualAddress;

        public GpuRingBuffer(uint maxSize, Device device)
            : base(maxSize)
        {
            var heapProperties = new HeapProperties(HeapType.Upload);
            var resourceDescription = ResourceDescription.Buffer(maxSize);
            var defaultUsage = ResourceStates.GenericRead;

            _buffer = device.CreateCommittedResource(
                heapProperties,
                HeapFlags.None,
                resourceDescription,
                defaultUsage);

            _buffer.Name = "Upload Ring Buffer";

            _gpuVirtualAddress = _buffer.GPUVirtualAddress;

            _cpuVirtualAddress = _buffer.Map(0);
        }

        public new DynamicAllocation? Allocate(uint sizeInBytes)
        {
            var offset = base.Allocate(sizeInBytes);
            if (offset == null)
            {
                return null;
            }

            var cpuAddress = (_cpuVirtualAddress != IntPtr.Zero)
                ? _cpuVirtualAddress + (int) offset.Value
                : IntPtr.Zero;

            return new DynamicAllocation
            {
                Buffer = _buffer,
                Offset = offset.Value,
                Size = sizeInBytes,
                GpuAddress = _gpuVirtualAddress + offset.Value,
                CpuAddress = cpuAddress
            };
        }

        public void Dispose()
        {
            _buffer.Unmap(0);

            _buffer.Dispose();
        }
    }
}
