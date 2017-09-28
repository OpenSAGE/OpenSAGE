namespace LLGfx
{
    internal sealed partial class ShaderResourceView : GraphicsDeviceChild
    {
        public static ShaderResourceView Create<T>(
            GraphicsDevice graphicsDevice, 
            StaticBuffer<T> buffer)
            where T : struct
        {
            var result = new ShaderResourceView(graphicsDevice, 1);

            result.PlatformSetStructuredBuffer(0, buffer);

            return result;
        }

        public static ShaderResourceView Create(
            GraphicsDevice graphicsDevice,
            Texture[] textures)
        {
            var result = new ShaderResourceView(graphicsDevice, textures.Length);

            for (var i = 0; i < textures.Length; i++)
            {
                result.PlatformSetTexture(i, textures[i]);
            }

            return result;
        }

        public static ShaderResourceView Create(
            GraphicsDevice graphicsDevice,
            Texture texture)
        {
            var result = new ShaderResourceView(graphicsDevice, 1);

            result.PlatformSetTexture(0, texture);

            return result;
        }

        private ShaderResourceView(GraphicsDevice graphicsDevice, int numResources)
            : base(graphicsDevice)
        {
            PlatformConstruct(graphicsDevice, numResources);
        }
    }
}
