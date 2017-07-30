using System;
using SharpDX;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class DynamicBuffer
    {
        private IntPtr _dataPointer;

        // TODO
        internal override long DeviceCurrentGPUVirtualAddress => throw new NotImplementedException();

        private void PlatformConstruct(GraphicsDevice graphicsDevice, uint sizeInBytes)
        {
            DeviceBuffer = AddDisposable(graphicsDevice.Device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(sizeInBytes),
                ResourceStates.GenericRead));

            _dataPointer = DeviceBuffer.Map(0);
        }

        private void PlatformSetData<T>(ref T data)
            where T : struct
        {
            // TODO

            var destinationPtr = DeviceBuffer.Map(0);
            Utilities.Write(destinationPtr, ref data);
            DeviceBuffer.Unmap(0);
        }

        protected override void Dispose(bool disposing)
        {
            DeviceBuffer.Unmap(0);

            base.Dispose(disposing);
        }
    }
}
