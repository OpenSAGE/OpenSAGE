namespace LLGfx
{
    public sealed partial class TextureSet : GraphicsDeviceChild
    {
        public Texture[] Textures { get; }

        public TextureSet(GraphicsDevice graphicsDevice, Texture[] textures)
            : base(graphicsDevice)
        {
            Textures = textures;

            PlatformConstruct(graphicsDevice, textures);
        }
    }
}
