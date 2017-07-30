using OpenZH.Graphics.Platforms.Direct3D12;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class Texture
    {
        internal Resource DeviceResource { get; private set; }

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            PixelFormat pixelFormat,
            int width,
            int height,
            TextureMipMapData[] mipMapData)
        {
            var resourceDescription = ResourceDescription.Texture2D(
                pixelFormat.ToDxgiFormat(),
                width,
                height,
                mipLevels: (short) mipMapData.Length);

            DeviceResource = AddDisposable(graphicsDevice.Device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                resourceDescription,
                ResourceStates.CopyDestination));

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
                uploadData);

            uploadBatch.Transition(
                DeviceResource,
                ResourceStates.CopyDestination,
                ResourceStates.NonPixelShaderResource | ResourceStates.PixelShaderResource);
        }
    }
}
