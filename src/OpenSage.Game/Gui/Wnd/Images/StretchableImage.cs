using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd.Images
{
    public sealed class StretchableImage : ImageBase
    {
        private readonly MappedImageTexture _left;
        private readonly MappedImageTexture _middle;
        private readonly MappedImageTexture _right;

        public StretchableImage(MappedImageTexture left, MappedImageTexture middle, MappedImageTexture right)
        {
            _left = left;
            _middle = middle;
            _right = right;
        }

        public override void Draw(DrawingContext2D drawingContext, in Rectangle destinationRect)
        {
            var leftWidth = _left.SourceRect.Width;
            var rightWidth = _right.SourceRect.Width;

            var leftRect = new Rectangle(0, 0, leftWidth, destinationRect.Height);

            drawingContext.DrawImage(
               _left.Texture,
               _left.SourceRect,
               leftRect);

            var middleRect = new Rectangle(
                leftRect.Right,
                0,
                destinationRect.Width - leftWidth - rightWidth,
                destinationRect.Height);

            drawingContext.DrawImage(
               _middle.Texture,
               _middle.SourceRect,
               middleRect);

            var rightRect = new Rectangle(
                middleRect.Right,
                0,
                rightWidth,
                destinationRect.Height);

            drawingContext.DrawImage(
               _right.Texture,
               _right.SourceRect,
               rightRect);
        }
    }
}
