using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using SharpDX;
using SharpDX.Direct3D12;

namespace LLGfx
{
    partial class ResourceUploadBatch
    {
        private GraphicsDevice _graphicsDevice;
        private Device _device;
        private List<Resource> _trackedResources;

        private CommandAllocator _commandAllocator;
        private GraphicsCommandList _commandList;

        private void PlatformConstruct(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _device = graphicsDevice.Device;

            _trackedResources = new List<Resource>();
        }

        private void PlatformBegin()
        {
            _commandAllocator = _device.CreateCommandAllocator(CommandListType.Direct);

            _commandList = _device.CreateCommandList(CommandListType.Direct, _commandAllocator, null);
        }

        internal void Upload<T>(Resource destinationResource, ResourceUploadData<T>[] sourceSubresourceData)
            where T : struct
        {
            if (!_inBeginEndBlock)
            {
                throw new InvalidOperationException();
            }

            var destinationResourceDescription = destinationResource.Description;
            var numSubresources = sourceSubresourceData.Length;

            var resourceLayouts = new PlacedSubResourceFootprint[numSubresources];
            var resourceNumRows = new int[numSubresources];
            var resourceRowSizesInBytes = new long[numSubresources];
            _device.GetCopyableFootprints(
                ref destinationResourceDescription,
                0,
                numSubresources,
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

            var sizeOfT = Marshal.SizeOf<T>();

            var dataPtr = uploadResource.Map(0);

            for (var i = 0; i < numSubresources; i++)
            {
                var subresourceData = sourceSubresourceData[i];

                var resourceLayout = resourceLayouts[i];
                var numRows = resourceNumRows[i];
                var resourceRowSizeInBytes = resourceRowSizesInBytes[i];

                for (var y = 0; y < numRows; y++)
                {
                    Utilities.Write(
                        dataPtr + (int) resourceLayout.Offset + (resourceLayout.Footprint.RowPitch * y),
                        subresourceData.Data,
                        (subresourceData.BytesPerRow / sizeOfT) * y,
                        (int) (resourceRowSizeInBytes / sizeOfT));
                }
            }

            uploadResource.Unmap(0);

            if (destinationResourceDescription.Dimension == ResourceDimension.Buffer)
            {
                _commandList.CopyBufferRegion(
                    destinationResource,
                    0,
                    uploadResource,
                    resourceLayouts[0].Offset,
                    resourceLayouts[0].Footprint.Width);
            }
            else
            {
                for (var i = 0; i < numSubresources; i++)
                {
                    _commandList.CopyTextureRegion(
                        new TextureCopyLocation(destinationResource, i),
                        0, 0, 0,
                        new TextureCopyLocation(uploadResource, resourceLayouts[i]),
                        null);
                }
            }

            _trackedResources.Add(uploadResource);
        }

        internal void Transition(Resource resource, ResourceStates stateBefore, ResourceStates stateAfter)
        {
            if (!_inBeginEndBlock)
            {
                throw new InvalidOperationException();
            }

            _commandList.ResourceBarrierTransition(
                resource,
                stateBefore,
                stateAfter);
        }

        private void PlatformEnd()
        {
            _commandList.Close();

            var d3d12CommandQueue = _graphicsDevice.CommandQueue.DeviceCommandQueue;
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
        }
    }

    internal struct ResourceUploadData<T>
        where T : struct
    {
        public T[] Data;
        public int BytesPerRow;
    }
}
