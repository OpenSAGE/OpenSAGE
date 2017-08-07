using System;
using System.Collections.Generic;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics.LowLevel.Util
{
    internal sealed class DescriptorTablePool : GraphicsObject
    {
        private readonly int _maxEntries;
        private readonly DescriptorHeap _descriptorHeap;
        private readonly int _incrementSize;

        private readonly Dictionary<int, DescriptorTablePoolEntry> _entries;

        private int _nextDescriptorIndex;

        public DescriptorHeap DeviceDescriptorHeap => _descriptorHeap;

        public DescriptorTablePool(Device device, DescriptorHeapType heapType, int maxEntries)
        {
            _maxEntries = maxEntries;

            _descriptorHeap = AddDisposable(device.CreateDescriptorHeap(new DescriptorHeapDescription
            {
                Type = heapType,
                Flags = DescriptorHeapFlags.ShaderVisible,
                DescriptorCount = maxEntries
            }));

            _incrementSize = device.GetDescriptorHandleIncrementSize(heapType);

            _entries = new Dictionary<int, DescriptorTablePoolEntry>();
        }

        public DescriptorTablePoolEntry Reserve(int descriptorCount)
        {
            if (_nextDescriptorIndex + descriptorCount > _maxEntries)
            {
                throw new InvalidOperationException();
            }

            var result = new DescriptorTablePoolEntry(
                _nextDescriptorIndex,
                descriptorCount,
                _descriptorHeap.GPUDescriptorHandleForHeapStart + (_incrementSize * descriptorCount),
                _descriptorHeap.CPUDescriptorHandleForHeapStart + (_incrementSize * descriptorCount),
                _incrementSize);

            _entries[_nextDescriptorIndex] = result;

            _nextDescriptorIndex += descriptorCount;

            return result;
        }

        public void Release(DescriptorTablePoolEntry entry)
        {
            // TODO: Do garbage collection

            _entries.Remove(entry.HeapIndex);
        }
    }

    internal sealed class DescriptorTablePoolEntry
    {
        private readonly int _incrementSize;

        public int HeapIndex { get; }

        public int DescriptorCount { get; }
        public GpuDescriptorHandle GpuDescriptorHandle { get; }
        public CpuDescriptorHandle CpuDescriptorHandle { get; }

        public DescriptorTablePoolEntry(
            int heapIndex,
            int descriptorCount,
            GpuDescriptorHandle gpuDescriptorHandle,
            CpuDescriptorHandle cpuDescriptorHandle,
            int incrementSize)
        {
            HeapIndex = heapIndex;
            DescriptorCount = descriptorCount;
            GpuDescriptorHandle = gpuDescriptorHandle;
            CpuDescriptorHandle = cpuDescriptorHandle;

            _incrementSize = incrementSize;
        }

        public CpuDescriptorHandle GetCpuHandle(int offset = 0)
        {
            return CpuDescriptorHandle + _incrementSize * offset;
        }
    }
}
