namespace LLGfx
{
    public sealed class TextureSet : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Texture[] _textures;

        private ShaderResourceView _shaderResourceView;

        internal ShaderResourceView ShaderResourceView
        {
            get
            {
                if (_shaderResourceView == null)
                {
                    _shaderResourceView = AddDisposable(ShaderResourceView.Create(
                        _graphicsDevice, 
                        _textures));
                }
                return _shaderResourceView;
            }
        }

        public TextureSet(GraphicsDevice graphicsDevice, Texture[] textures)
        {
            _graphicsDevice = graphicsDevice;
            _textures = textures;
        }
    }
}
