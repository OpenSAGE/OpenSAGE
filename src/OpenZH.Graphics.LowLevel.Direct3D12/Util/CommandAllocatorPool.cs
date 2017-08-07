using System;
using System.Collections.Generic;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics.LowLevel.Util
{
    internal sealed class CommandAllocatorPool : GraphicsObject
    {
        private readonly Device _device;
        private readonly CommandListType _commandListType;
        private readonly List<CommandAllocator> _allocatorPool;
        private readonly Queue<Tuple<long, CommandAllocator>> _readyAllocators;
        private readonly object _lockObject = new object();

        public CommandAllocatorPool(GraphicsDevice graphicsDevice, CommandListType commandListType)
        {
            _device = graphicsDevice.Device;
            _commandListType = commandListType;

            _allocatorPool = new List<CommandAllocator>();
            _readyAllocators = new Queue<Tuple<long, CommandAllocator>>();
        }

        public CommandAllocator AcquireAllocator(long completedFenceValue)
        {
            lock (_lockObject)
            {
                CommandAllocator allocator = null;

                // Try to find available allocator.
                if (_readyAllocators.Count > 0)
                {
                    var allocatorTuple = _readyAllocators.Peek();

                    if (allocatorTuple.Item1 <= completedFenceValue)
                    {
                        allocator = allocatorTuple.Item2;
                        allocator.Reset();
                        _readyAllocators.Dequeue();
                    }
                }

                // Otherwise, create a new one.
                if (allocator == null)
                {
                    allocator = AddDisposable(_device.CreateCommandAllocator(_commandListType));
                    _allocatorPool.Add(allocator);
                }

                return allocator;
            }
        }

        public void ReleaseAllocator(long fenceValue, CommandAllocator allocator)
        {
            lock (_lockObject)
            {
                _readyAllocators.Enqueue(Tuple.Create(fenceValue, allocator));
            }
        }
    }
}
