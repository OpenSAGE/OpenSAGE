using System;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Elements
{
    public sealed class StretchableImage
    {
        private readonly StretchableImageMode _mode;

        private readonly CroppedBitmap _croppedBitmapLeft;
        private readonly CroppedBitmap _croppedBitmapMiddle;
        private readonly CroppedBitmap _croppedBitmapRight;

        private readonly int _totalWidth;

        public StretchableImage(CroppedBitmap croppedBitmap)
        {
            _mode = StretchableImageMode.Normal;

            _croppedBitmapMiddle = croppedBitmap;
        }

        public StretchableImage(
            CroppedBitmap croppedBitmapLeft,
            CroppedBitmap croppedBitmapMiddle,
            CroppedBitmap croppedBitmapRight)
        {
            _mode = StretchableImageMode.Stretchable;

            _croppedBitmapLeft = croppedBitmapLeft;
            _croppedBitmapMiddle = croppedBitmapMiddle;
            _croppedBitmapRight = croppedBitmapRight;

            _totalWidth = croppedBitmapLeft.SourceRect.Width + croppedBitmapMiddle.SourceRect.Width + croppedBitmapRight.SourceRect.Width;
        }

        public void Render(DrawingContext drawingContext, in Rectangle destinationRect)
        {
            switch (_mode)
            {
                case StretchableImageMode.Normal:
                    drawingContext.DrawImage(
                       _croppedBitmapMiddle.Bitmap,
                       destinationRect,
                       _croppedBitmapMiddle.SourceRect);
                    break;

                case StretchableImageMode.Stretchable:
                    var widthRatio = destinationRect.Width / (float) _totalWidth;
                    var leftRect = new Rectangle(destinationRect.X, destinationRect.Y, (int) (_croppedBitmapLeft.SourceRect.Width * widthRatio), destinationRect.Height);
                    drawingContext.DrawImage(
                       _croppedBitmapLeft.Bitmap,
                       leftRect,
                       _croppedBitmapLeft.SourceRect);
                    var middleRect = new Rectangle(leftRect.Right, destinationRect.Y, (int) (_croppedBitmapMiddle.SourceRect.Width * widthRatio), destinationRect.Height);
                    drawingContext.DrawImage(
                       _croppedBitmapMiddle.Bitmap,
                       middleRect,
                       _croppedBitmapMiddle.SourceRect);
                    var rightRect = new Rectangle(middleRect.Right, destinationRect.Y, (int) (_croppedBitmapRight.SourceRect.Width * widthRatio), destinationRect.Height);
                    drawingContext.DrawImage(
                       _croppedBitmapRight.Bitmap,
                       rightRect,
                       _croppedBitmapRight.SourceRect);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public enum StretchableImageMode
    {
        Normal,
        Stretchable
    }
}
