namespace OpenZH.Graphics
{
    partial class DescriptorSet
    {
        private void PlatformConstruct(GraphicsDevice graphicsDevice, DescriptorSetLayout layout) { }

        private void PlatformSetConstantBuffer(int index, StaticBuffer buffer) { }

        private void PlatformSetStructuredBuffer(int index, StaticBuffer buffer) { }

        private void PlatformSetTypedBuffer(int index, StaticBuffer buffer, PixelFormat format) { }

        private void PlatformSetTexture(int index, Texture texture) { }
    }
}
