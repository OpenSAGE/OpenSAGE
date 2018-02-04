using System;
using System.IO;
using System.Linq;
using OpenSage.Content;
using OpenSage.Content.Util;
using OpenSage.Data.Wnd;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd
{
    public sealed class StretchableImage
    {
        private readonly StretchableImageMode _mode;

        private readonly CroppedBitmap _croppedBitmapLeft;
        private readonly CroppedBitmap _croppedBitmapMiddle;
        private readonly CroppedBitmap _croppedBitmapRight;

        private readonly int _totalWidth;
        private readonly int _height;

        public static StretchableImage CreateNormalImage(
            WndWindowDefinition wndWindow,
            WndDrawData wndDrawData,
            ContentManager contentManager)
        {
            var image = LoadImage(wndDrawData, 0, contentManager);
            return image != null
                ? CreateNormal(wndWindow.ScreenRect.ToRectangle().Width, image)
                : null;
        }

        public static StretchableImage CreatePushButtonImage(
            WndWindowDefinition wndWindow,
            WndDrawData wndDrawData,
            ContentManager contentManager,
            int leftIndex,
            int middleIndex,
            int rightIndex)
        {
            var imageLeft = LoadImage(wndDrawData, leftIndex, contentManager);
            var imageMiddle = LoadImage(wndDrawData, middleIndex, contentManager);
            var imageRight = LoadImage(wndDrawData, rightIndex, contentManager);

            if (imageLeft != null && imageMiddle != null && imageRight != null)
                return CreateStretchable(wndWindow.ScreenRect.ToRectangle().Width, imageLeft, imageMiddle, imageRight);

            //if (imageLeft != null)
            //    return StretchableImage.CreateNormal(wndWindow.ScreenRect.ToRectangle().Width, imageLeft);

            return null;
        }

        private static CroppedBitmap LoadImage(WndDrawData wndDrawData, int drawDataIndex, ContentManager contentManager)
        {
            var image = wndDrawData.Items[drawDataIndex].Image;

            if (string.IsNullOrEmpty(image) || image == "NoImage")
            {
                return null;
            }

            var mappedImage = contentManager.IniDataContext.MappedImages.FirstOrDefault(x => x.Name == image);
            if (mappedImage == null)
            {
                return null;
            }

            var texture = contentManager.Load<Texture>(
                new[]
                {
                    Path.Combine(@"Data\English\Art\Textures", mappedImage.Texture),
                    Path.Combine(@"Art\Textures", mappedImage.Texture)
                },
                new TextureLoadOptions { GenerateMipMaps = false });

            var textureRect = mappedImage.Coords.ToRectangle();

            return new CroppedBitmap(texture, textureRect);
        }

        private StretchableImage(
            int destinationWidth,
            StretchableImageMode mode,
            CroppedBitmap croppedBitmapLeft,
            CroppedBitmap croppedBitmapMiddle,
            CroppedBitmap croppedBitmapRight)
        {
            _totalWidth = destinationWidth;

            _mode = mode;

            _croppedBitmapLeft = croppedBitmapLeft;
            _croppedBitmapMiddle = croppedBitmapMiddle;
            _croppedBitmapRight = croppedBitmapRight;

            _height = croppedBitmapLeft.SourceRect.Height;
        }

        private static StretchableImage CreateNormal(int destinationWidth, CroppedBitmap croppedBitmap)
        {
            return new StretchableImage(destinationWidth, StretchableImageMode.Normal, croppedBitmap, null, null);
        }

        private static StretchableImage CreateStretchable(
            int destinationWidth,
            CroppedBitmap croppedBitmapLeft,
            CroppedBitmap croppedBitmapMiddle,
            CroppedBitmap croppedBitmapRight)
        {
            return new StretchableImage(
                destinationWidth,
                StretchableImageMode.Stretchable,
                croppedBitmapLeft,
                croppedBitmapMiddle,
                croppedBitmapRight);
        }

        internal Texture RenderToTexture(ContentManager contentManager)
        {
            var imageTexture = contentManager.GraphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    (uint) _totalWidth,
                    (uint) _height,
                    1,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled | TextureUsage.RenderTarget));

            imageTexture.Name = "StretchableImage";

            using (var drawingContext = new DrawingContext2D(contentManager, imageTexture))
            {
                drawingContext.Begin(
                    contentManager.PointClampSampler,
                    ColorRgbaF.Transparent);

                switch (_mode)
                {
                    case StretchableImageMode.Normal:
                        drawingContext.DrawImage(
                           _croppedBitmapLeft.Bitmap,
                           _croppedBitmapLeft.SourceRect,
                           new Rectangle(0, 0, _totalWidth, _height));
                        break;

                    case StretchableImageMode.Stretchable:
                        var leftWidth = _croppedBitmapLeft.SourceRect.Width;
                        var rightWidth = _croppedBitmapRight.SourceRect.Width;
                        var leftRect = new Rectangle(0, 0, leftWidth, _height);
                        drawingContext.DrawImage(
                           _croppedBitmapLeft.Bitmap,
                           _croppedBitmapLeft.SourceRect,
                           leftRect);
                        var middleRect = new Rectangle(leftRect.Right, 0, _totalWidth - leftWidth - rightWidth, _height);
                        drawingContext.DrawImage(
                           _croppedBitmapMiddle.Bitmap,
                           _croppedBitmapMiddle.SourceRect,
                           middleRect);
                        var rightRect = new Rectangle(middleRect.Right, 0, rightWidth, _height);
                        drawingContext.DrawImage(
                           _croppedBitmapRight.Bitmap,
                           _croppedBitmapRight.SourceRect,
                           rightRect);
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                drawingContext.End();
            }

            return imageTexture;
        }
    }

    public enum StretchableImageMode
    {
        Normal,
        Stretchable
    }
}
