using System;
using OpenSage.Content;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd.Images
{
    internal abstract class CachedImageSource : ImageSource
    {
        private readonly ImageTextureCache _cache;

        protected abstract string CacheKey { get; }

        protected CachedImageSource(ImageTextureCache cache)
        {
            _cache = cache;
        }

        public override Texture GetTexture(Size size)
        {
            return _cache.GetOrCreateTexture(CacheKey, size, context => CreateTexture(size, context));
        }

        protected abstract Texture CreateTexture(Size size, GraphicsLoadContext loadContext);
    }
}
