using OpenSage.Content;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd.Images
{
    internal sealed class MappedImageSource : CachedImageSource
    {
        private readonly MappedImage _mappedImage;

        public override Size NaturalSize => _mappedImage.Coords.Size;

        protected override string CacheKey => _mappedImage.FullName;

        public MappedImageSource(MappedImage mappedImage, ImageTextureCache cache)
            : base(cache)
        {
            _mappedImage = mappedImage;
        }

        protected override Texture CreateTexture(Size size, GraphicsLoadContext loadContext)
        {
            return MappedImageUtility.CreateTexture(loadContext, _mappedImage); 
        }
    }
}
