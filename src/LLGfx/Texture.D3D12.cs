using LLGfx.Util;
using SharpDX.Direct3D12;

namespace LLGfx
{
    partial class Texture
    {
        internal Resource DeviceResource { get; private set; }

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            PixelFormat pixelFormat,
            int arraySize,
            int width,
            int height,
            int mipMapCount)
        {
            var resourceDescription = ResourceDescription.Texture2D(
                pixelFormat.ToDxgiFormat(),
                width,
                height,
                arraySize: (short) arraySize,
                mipLevels: (short) mipMapCount);

            DeviceResource = AddDisposable(graphicsDevice.Device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                resourceDescription,
                ResourceStates.CopyDestination));
        }

        private void PlatformSetData(
            ResourceUploadBatch uploadBatch,
            int arrayIndex, 
            TextureMipMapData[] mipMapData)
        {
            var uploadData = new ResourceUploadData<byte>[mipMapData.Length];
            for (var i = 0; i < mipMapData.Length; i++)
            {
                uploadData[i] = new ResourceUploadData<byte>
                {
                    Data = mipMapData[i].Data,
                    BytesPerRow = mipMapData[i].BytesPerRow
                };
            }

            uploadBatch.Upload(
                DeviceResource,
                arrayIndex,
                uploadData);
        }

        private void PlatformFreeze(ResourceUploadBatch uploadBatch)
        {
            uploadBatch.Transition(
                DeviceResource,
                ResourceStates.CopyDestination,
                ResourceStates.NonPixelShaderResource | ResourceStates.PixelShaderResource);
        }
    }
}
