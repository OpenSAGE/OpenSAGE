using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd.Images
{
    public sealed class Image : ImageBase
    {
        private readonly MappedImageTexture _texture;

        public Image(MappedImageTexture texture)
        {
            _texture = texture;
        }

        public override void Draw(DrawingContext2D drawingContext, in Rectangle destinationRect)
        {
            drawingContext.DrawImage(_texture.Texture, _texture.SourceRect, destinationRect);
        }
    }
}
