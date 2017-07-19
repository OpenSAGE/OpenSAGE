using SharpDX.Direct3D12;

namespace OpenZH.Graphics.Direct3D12
{
    public sealed class D3D12Texture : Texture
    {
        private readonly D3D12GraphicsDevice _graphicsDevice;

        public Resource Resource { get; }

        public D3D12Texture(D3D12GraphicsDevice graphicsDevice, ResourceDescription description)
        {
            _graphicsDevice = graphicsDevice;

            Resource = AddDisposable(graphicsDevice.Device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                description,
                ResourceStates.CopyDestination));
        }

        public override void SetData(ResourceUploadBatch uploadBatch, int level, byte[] data, int bytesPerRow)
        {
            var d3d12UploadBatch = (D3D12ResourceUploadBatch) uploadBatch;

            d3d12UploadBatch.Upload(Resource, level, data, bytesPerRow);

            d3d12UploadBatch.Transition(
                Resource,
                level,
                ResourceStates.CopyDestination,
                ResourceStates.PixelShaderResource);
        }
    }
}
