namespace LLGfx
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

        public void SetStructuredBuffer<T>(int index, StaticBuffer<T> buffer)
            where T : struct
        {
            // TODO: Validation.

            PlatformSetStructuredBuffer(index, buffer);
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
