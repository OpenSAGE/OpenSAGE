using System.Threading;
using OpenZH.Graphics.Direct3D12.Util;
using SharpDX.Direct3D12;
using D3D12 = SharpDX.Direct3D12;

namespace OpenZH.Graphics.Direct3D12
{
    public sealed class D3D12CommandQueue : CommandQueue
    {
        private readonly Device _device;
        private readonly D3D12.CommandQueue _commandQueue;
        private readonly CommandAllocatorPool _allocatorPool;

        private readonly D3D12CommandBuffer _commandBuffer;

        private readonly object _fenceLock = new object();
        private readonly object _eventLock = new object();

        private readonly Fence _fence;
        private readonly AutoResetEvent _fenceEvent;
        private long _nextFenceValue;
        private long _lastCompletedFenceValue;

        private GraphicsCommandList _commandList;
        private CommandAllocator _currentAllocator;

        public D3D12.CommandQueue DeviceCommandQueue => _commandQueue;

        internal D3D12CommandQueue(Device device)
        {
            _device = device;

            const CommandListType commandListType = CommandListType.Direct;

            _commandQueue = device.CreateCommandQueue(new CommandQueueDescription(commandListType));

            _fence = device.CreateFence(0, FenceFlags.None);

            _fenceEvent = new AutoResetEvent(false);

            _allocatorPool = new CommandAllocatorPool(_device, commandListType);

            _nextFenceValue = 1;
            _lastCompletedFenceValue = 0;

            _commandBuffer = new D3D12CommandBuffer(this);
        }

        internal GraphicsCommandList GetOrCreateCommandList()
        {
            // For now, just create a single command list. We can expand
            // this in the future.

            var completedFence = _fence.CompletedValue;
            _currentAllocator = _allocatorPool.AcquireAllocator(completedFence);

            if (_commandList != null)
            {
                _commandList.Reset(
                    _currentAllocator,
                    null);
            }
            else
            {
                _commandList = _device.CreateCommandList(
                    _commandQueue.Description.Type,
                    _currentAllocator,
                    null);
            }

            return _commandList;
        }

        public override CommandBuffer GetCommandBuffer()
        {
            return _commandBuffer;
        }

        internal void ExecuteCommandList()
        {
            var commandList = _commandList;

            long fenceValue;

            lock (_fenceLock)
            {
                commandList.Close();

                _commandQueue.ExecuteCommandList(commandList);

                _commandQueue.Signal(_fence, _nextFenceValue);

                fenceValue = _nextFenceValue;

                _nextFenceValue++;
            }

            _allocatorPool.ReleaseAllocator(fenceValue, _currentAllocator);
        }

        protected override void Dispose(bool disposing)
        {
            _allocatorPool.Dispose();
            _fenceEvent.Dispose();
            _fence.Dispose();
            _commandQueue.Dispose();

            base.Dispose(disposing);
        }
    }
}
