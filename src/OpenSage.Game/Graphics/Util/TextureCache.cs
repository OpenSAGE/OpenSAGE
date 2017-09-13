using System.Collections.Generic;
using LLGfx;
using OpenSage.Data;

namespace OpenSage.Graphics.Util
{
    public sealed class TextureCache : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<FileSystemEntry, Texture> _cachedTextures;
        private Texture _placeholderTexture;

        public TextureCache(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _cachedTextures = new Dictionary<FileSystemEntry, Texture>();
        }

        public Texture GetTexture(FileSystemEntry entry, ResourceUploadBatch uploadBatch)
        {
            if (!_cachedTextures.TryGetValue(entry, out var result))
            {
                _cachedTextures[entry] = result = AddDisposable(TextureLoader.LoadTexture(
                    _graphicsDevice,
                    uploadBatch,
                    entry,
                    true));
            }
            return result;
        }

        public Texture GetPlaceholderTexture(ResourceUploadBatch uploadBatch)
        {
            if (_placeholderTexture == null)
            {
                _placeholderTexture = AddDisposable(Texture.CreatePlaceholderTexture2D(
                    _graphicsDevice,
                    uploadBatch));
            }
            return _placeholderTexture;
        }
    }
}
