using System;
using System.Collections.Generic;
using System.Threading;
using OpenZH.Graphics.Direct3D12.Util;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics.Direct3D12
{
    public sealed class D3D12ResourceUploadBatch : ResourceUploadBatch
    {
        private readonly Device _device;
        private readonly List<Resource> _trackedResources;

        private bool _inBeginEndBlock;

        private CommandAllocator _commandAllocator;
        private GraphicsCommandList _commandList;

        internal D3D12ResourceUploadBatch(Device device)
        {
            _device = device;

            _trackedResources = new List<Resource>();
        }

        public override void Begin()
        {
            if (_inBeginEndBlock)
            {
                throw new InvalidOperationException();
            }

            _commandAllocator = _device.CreateCommandAllocator(CommandListType.Direct);

            _commandList = _device.CreateCommandList(CommandListType.Direct, _commandAllocator, null);

            _inBeginEndBlock = true;
        }

        internal void Upload(Resource resource, int subresource, byte[] data, int bytesPerRow)
        {
            if (!_inBeginEndBlock)
            {
                throw new InvalidOperationException();
            }

            var resourceDescription = resource.Description;

            var resourceLayouts = new PlacedSubResourceFootprint[1];
            var resourceNumRows = new int[1];
            var resourceRowSizesInBytes = new long[1];
            _device.GetCopyableFootprints(
                ref resourceDescription,
                subresource,
                1,
                0,
                resourceLayouts,
                resourceNumRows,
                resourceRowSizesInBytes,
                out var uploadSize);

            // Create intermediate GPU upload buffer.
            var uploadResource = _device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(uploadSize),
                ResourceStates.GenericRead);

            var dataPtr = uploadResource.Map(0);

            ResourceUploadUtil.MemcpySubresource(
                dataPtr,
                resourceLayouts[0].Footprint.RowPitch,
                data,
                bytesPerRow,
                (int) resourceRowSizesInBytes[0],
                resourceNumRows[0]);

            uploadResource.Unmap(0);

            _commandList.CopyTextureRegion(
                new TextureCopyLocation(resource, subresource),
                0, 0, 0,
                new TextureCopyLocation(uploadResource, resourceLayouts[0]),
                null);

            _trackedResources.Add(uploadResource);
        }

        internal void Transition(Resource resource, int subresource, ResourceStates stateBefore, ResourceStates stateAfter)
        {
            if (!_inBeginEndBlock)
            {
                throw new InvalidOperationException();
            }

            _commandList.ResourceBarrierTransition(
                resource,
                subresource,
                stateBefore,
                stateAfter);
        }

        public override void End(CommandQueue commandQueue)
        {
            if (!_inBeginEndBlock)
            {
                throw new InvalidOperationException();
            }

            _commandList.Close();

            var d3d12CommandQueue = ((D3D12CommandQueue) commandQueue).DeviceCommandQueue;
            d3d12CommandQueue.ExecuteCommandList(_commandList);

            using (var fence = _device.CreateFence(0, FenceFlags.None))
            using (var gpuCompletedEvent = new ManualResetEvent(false))
            {
                d3d12CommandQueue.Signal(fence, 1);
                fence.SetEventOnCompletion(1, gpuCompletedEvent.GetSafeWaitHandle().DangerousGetHandle());

                gpuCompletedEvent.WaitOne();
            }

            foreach (var resource in _trackedResources)
            {
                resource.Dispose();
            }
            _trackedResources.Clear();

            _commandList.Dispose();
            _commandList = null;

            _commandAllocator.Dispose();
            _commandAllocator = null;

            _inBeginEndBlock = false;
        }
    }
}
