using OpenSage.Content;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd.Images
{
    internal sealed class StretchableMappedImageSource : CachedImageSource
    {
        private readonly MappedImage _left, _middle, _right;

        public override Size NaturalSize { get; }

        protected override string CacheKey { get; }

        public StretchableMappedImageSource(MappedImage left, MappedImage middle, MappedImage right, ImageTextureCache cache)
            : base(cache)
        {
            _left = left;
            _middle = middle;
            _right = right;

            NaturalSize = new Size(
                _left.Coords.Width + _middle.Coords.Width + _right.Coords.Width,
                _left.Coords.Height);

            CacheKey = $"Stretchable:{left.Name}:{middle.Name}:{right.Name}";
        }

        protected override Texture CreateTexture(Size size, GraphicsLoadContext loadContext)
        {
            return MappedImageUtility.CreateTexture(loadContext, size, spriteBatch =>
            {
                var requiresFlip = !loadContext.GraphicsDevice.IsUvOriginTopLeft;

                var leftWidth = _left.Coords.Width;
                var rightWidth = _right.Coords.Width;

                var leftRect = new Rectangle(0, 0, leftWidth, size.Height);

                spriteBatch.DrawImage(
                   _left.Texture.Value,
                   _left.Coords,
                   leftRect.ToRectangleF(),
                   ColorRgbaF.White,
                   requiresFlip);

                var middleRect = new Rectangle(leftRect.Right, 0, size.Width - leftWidth - rightWidth, size.Height);
                spriteBatch.DrawImage(
                   _middle.Texture.Value,
                   _middle.Coords,
                   middleRect.ToRectangleF(),
                   ColorRgbaF.White,
                   requiresFlip);

                var rightRect = new Rectangle(middleRect.Right, 0, rightWidth, size.Height);
                spriteBatch.DrawImage(
                   _right.Texture.Value,
                   _right.Coords,
                   rightRect.ToRectangleF(),
                   ColorRgbaF.White,
                   requiresFlip);
            });
        }
    }
}
