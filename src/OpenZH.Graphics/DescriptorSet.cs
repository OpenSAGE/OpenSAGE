namespace OpenZH.Graphics
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

        public void SetConstantBuffers(int startIndex, Buffer[] buffers)
        {
            for (var i = 0; i < buffers.Length; i++)
            {
                var buffer = buffers[i];

                SetConstantBuffer(startIndex + i, buffer);
            }
        }

        public void SetConstantBuffer(int index, Buffer buffer)
        {
            // TODO: Validation.

            PlatformSetConstantBuffer(index, buffer);
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
