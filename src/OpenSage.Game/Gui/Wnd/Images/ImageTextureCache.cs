using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd.Images
{
    internal sealed class ImageTextureCache : DisposableBase
    {
        private readonly Dictionary<CacheKey, Texture> _cachedTextures = new Dictionary<CacheKey, Texture>();
        private readonly GraphicsLoadContext _loadContext;

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            public readonly string SourceKey;
            public readonly Size Size;

            public CacheKey(string sourceKey, Size size)
            {
                SourceKey = sourceKey;
                Size = size;
            }

            bool IEquatable<CacheKey>.Equals(CacheKey other)
            {
                return SourceKey == other.SourceKey && Size == other.Size;
            }
        }

        public ImageTextureCache(GraphicsLoadContext loadContext)
        {
            _loadContext = loadContext;
        }

        public Texture GetOrCreateTexture(string sourceKey, Size size, Func<GraphicsLoadContext, Texture> createTexture)
        {
            var cacheKey = new CacheKey(sourceKey, size);
            if (!_cachedTextures.TryGetValue(cacheKey, out var result))
            {
                result = AddDisposable(createTexture(_loadContext));
                _cachedTextures.Add(cacheKey, result);
            }
            return result;
        }
    }
}
