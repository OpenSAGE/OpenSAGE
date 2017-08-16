using System;
using System.Collections.Generic;
using SharpDX.Direct3D12;

namespace LLGfx.Util
{
    internal sealed class DynamicUploadHeap : IDisposable
    {
        /// <summary>
        /// Default alignment for constant buffers, as required by D3D12.
        /// </summary>
        public const uint DefaultAlignment = 256;

        private readonly Device _device;
        private readonly List<GpuRingBuffer> _ringBuffers;

        public DynamicUploadHeap(Device device, uint initialSize)
        {
            _device = device;

            _ringBuffers = new List<GpuRingBuffer>();
            _ringBuffers.Add(new GpuRingBuffer(initialSize, device));
        }

        public DynamicAllocation Allocate(uint sizeInBytes, uint alignment = DefaultAlignment)
        {
            var alignmentMask = alignment - 1;

            if ((alignmentMask & alignment) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(alignment));
            }

            var alignedSize = (sizeInBytes + alignmentMask) & ~alignmentMask;

            var dynamicAllocationTemp = _ringBuffers[_ringBuffers.Count - 1].Allocate(alignedSize);

            DynamicAllocation dynamicAllocation;
            if (dynamicAllocationTemp != null)
            {
                dynamicAllocation = dynamicAllocationTemp.Value;
            }
            else
            {
                var newMaxSize = _ringBuffers[_ringBuffers.Count - 1].MaxSize * 2;
                while (newMaxSize < sizeInBytes)
                {
                    newMaxSize *= 2;
                }
                GpuRingBuffer newRingBuffer;
                _ringBuffers.Add(newRingBuffer = new GpuRingBuffer(newMaxSize, _device));
                dynamicAllocation = newRingBuffer.Allocate(alignedSize).Value;
            }

            return dynamicAllocation;
        }

        public void FinishFrame(long frameNumber, long numCompletedFrames)
        {
            var numBuffersToDelete = 0;

            for (var i = 0; i < _ringBuffers.Count; i++)
            {
                var ringBuffer = _ringBuffers[i];

                ringBuffer.FinishCurrentFrame(frameNumber);
                ringBuffer.ReleaseCompletedFrames(numCompletedFrames);

                if (numBuffersToDelete == i && i < _ringBuffers.Count - 1 && ringBuffer.IsEmpty)
                {
                    ringBuffer.Dispose();
                    numBuffersToDelete += 1;
                }
            }

            if (numBuffersToDelete > 0)
            {
                _ringBuffers.RemoveRange(0, numBuffersToDelete);
            }
        }

        public void Dispose()
        {
            foreach (var ringBuffer in _ringBuffers)
            {
                ringBuffer.Dispose();
            }
        }
    }
}
