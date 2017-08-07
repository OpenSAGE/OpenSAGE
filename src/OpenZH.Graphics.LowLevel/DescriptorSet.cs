namespace OpenZH.Graphics.LowLevel
{
    public sealed partial class DescriptorSet : GraphicsDeviceChild
    {
        public DescriptorSetLayout Layout { get; }

        public DescriptorSet(GraphicsDevice graphicsDevice, DescriptorSetLayout layout)
            : base(graphicsDevice)
        {
            Layout = layout;

            PlatformConstruct(graphicsDevice, layout);
        }

        public void SetConstantBuffers(int startIndex, StaticBuffer[] buffers)
        {
            for (var i = 0; i < buffers.Length; i++)
            {
                var buffer = buffers[i];

                SetConstantBuffer(startIndex + i, buffer);
            }
        }

        public void SetConstantBuffer(int index, StaticBuffer buffer)
        {
            // TODO: Validation.

            PlatformSetConstantBuffer(index, buffer);
        }

        public void SetStructuredBuffer(int index, StaticBuffer buffer)
        {
            // TODO: Validation.

            PlatformSetStructuredBuffer(index, buffer);
        }

        public void SetTypedBuffer(int index, StaticBuffer buffer, PixelFormat format)
        {
            // TODO: Validation.
            PlatformSetTypedBuffer(index, buffer, format);
        }

        public void SetTextures(int startIndex, Texture[] textures)
        {
            for (var i = 0; i < textures.Length; i++)
            {
                var texture = textures[i];

                SetTexture(startIndex + i, texture);
            }
        }

        public void SetTexture(int index, Texture texture)
        {
            // TODO: Validation.

            PlatformSetTexture(index, texture);
        }
    }
}
