using System.Threading;
using SharpDX.Direct3D12;
using D3D12 = SharpDX.Direct3D12;
using OpenZH.Graphics.LowLevel.Util;

namespace OpenZH.Graphics.LowLevel
{
    partial class CommandQueue
    {
        private CommandAllocatorPool _allocatorPool;

        private CommandBuffer _commandBuffer;

        private readonly object _fenceLock = new object();
        private readonly object _eventLock = new object();

        private Fence _fence;
        private AutoResetEvent _fenceEvent;
        private long _nextFenceValue;
        private long _lastCompletedFenceValue;

        private GraphicsCommandList _commandList;
        private CommandAllocator _currentAllocator;

        internal D3D12.CommandQueue DeviceCommandQueue { get; private set; }

        private void PlatformConstruct(GraphicsDevice graphicsDevice)
        {
            const CommandListType commandListType = CommandListType.Direct;

            var device = graphicsDevice.Device;

            DeviceCommandQueue = AddDisposable(device.CreateCommandQueue(new CommandQueueDescription(commandListType)));

            _fence = AddDisposable(device.CreateFence(0, FenceFlags.None));

            _fenceEvent = AddDisposable(new AutoResetEvent(false));

            _allocatorPool = AddDisposable(new CommandAllocatorPool(graphicsDevice, commandListType));

            _nextFenceValue = 1;
            _lastCompletedFenceValue = 0;

            _commandBuffer = AddDisposable(new CommandBuffer(graphicsDevice, this));
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
                _commandList = GraphicsDevice.Device.CreateCommandList(
                    DeviceCommandQueue.Description.Type,
                    _currentAllocator,
                    null);
            }

            _commandList.SetDescriptorHeaps(GraphicsDevice.DescriptorHeapCbvUavSrv.DeviceDescriptorHeap);

            return _commandList;
        }

        private CommandBuffer PlatformGetCommandBuffer()
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

                DeviceCommandQueue.ExecuteCommandList(commandList);

                DeviceCommandQueue.Signal(_fence, _nextFenceValue);

                fenceValue = _nextFenceValue;

                _nextFenceValue++;
            }

            _allocatorPool.ReleaseAllocator(fenceValue, _currentAllocator);
        }
    }
}
