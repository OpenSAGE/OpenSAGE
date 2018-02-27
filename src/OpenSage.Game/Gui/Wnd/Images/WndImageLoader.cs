using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Wnd;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd.Images
{
    internal sealed class WndImageLoader : DisposableBase
    {
        private struct WndImageKey
        {
            public Size DestinationSize;
            public string LeftImage;
            public string MiddleImage;
            public string RightImage;
        }

        private readonly ContentManager _contentManager;
        private readonly MappedImageLoader _mappedImageManager;
        private readonly Dictionary<WndImageKey, Texture> _cache;

        public WndImageLoader(ContentManager contentManager, MappedImageLoader mappedImageManager)
        {
            _contentManager = contentManager;
            _mappedImageManager = mappedImageManager;
            _cache = new Dictionary<WndImageKey, Texture>();
        }

        public Image CreateNormalImage(
            WndDrawData wndDrawData,
            int index)
        {
            var leftImage = wndDrawData.Items[index].Image;
            var mappedImageTexture = GetMappedImage(leftImage);

            if (mappedImageTexture != null)
            {
                return new Image(mappedImageTexture.SourceRect.Size, size =>
                {
                    var cacheKey = new WndImageKey
                    {
                        DestinationSize = size,
                        LeftImage = leftImage
                    };

                    if (!_cache.TryGetValue(cacheKey, out var result))
                    {
                        result = CreateTexture(
                            cacheKey,
                            drawingContext =>
                            {
                                drawingContext.DrawImage(
                                    mappedImageTexture.Texture,
                                    mappedImageTexture.SourceRect,
                                    new Rectangle(Point2D.Zero, size));
                            });

                        _cache.Add(cacheKey, result);
                    }

                    return result;
                });
            }
            else
            {
                return null;
            }
        }

        public Image CreateStretchableImage(
            WndDrawData wndDrawData,
            int leftIndex,
            int middleIndex,
            int rightIndex)
        {
            var leftImage = wndDrawData.Items[leftIndex].Image;
            var middleImage = wndDrawData.Items[middleIndex].Image;
            var rightImage = wndDrawData.Items[rightIndex].Image;

            var leftMappedImageTexture = GetMappedImage(leftImage);
            var middleMappedImageTexture = GetMappedImage(middleImage);
            var rightMappedImageTexture = GetMappedImage(rightImage);

            if (leftMappedImageTexture != null &&
                middleMappedImageTexture != null &&
                rightMappedImageTexture != null)
            {
                var naturalSize = new Size(
                    leftMappedImageTexture.SourceRect.Width + middleMappedImageTexture.SourceRect.Width + rightMappedImageTexture.SourceRect.Width,
                    leftMappedImageTexture.SourceRect.Height);

                return new Image(naturalSize, size =>
                {
                    var cacheKey = new WndImageKey
                    {
                        DestinationSize = size,
                        LeftImage = leftImage,
                        MiddleImage = middleImage,
                        RightImage = rightImage
                    };

                    if (!_cache.TryGetValue(cacheKey, out var result))
                    {
                        result = CreateTexture(
                            cacheKey,
                            drawingContext =>
                            {
                                var leftWidth = leftMappedImageTexture.SourceRect.Width;
                                var rightWidth = rightMappedImageTexture.SourceRect.Width;
                                var leftRect = new Rectangle(0, 0, leftWidth, cacheKey.DestinationSize.Height);
                                drawingContext.DrawImage(
                                   leftMappedImageTexture.Texture,
                                   leftMappedImageTexture.SourceRect,
                                   leftRect);
                                var middleRect = new Rectangle(leftRect.Right, 0, cacheKey.DestinationSize.Width - leftWidth - rightWidth, cacheKey.DestinationSize.Height);
                                drawingContext.DrawImage(
                                   middleMappedImageTexture.Texture,
                                   middleMappedImageTexture.SourceRect,
                                   middleRect);
                                var rightRect = new Rectangle(middleRect.Right, 0, rightWidth, cacheKey.DestinationSize.Height);
                                drawingContext.DrawImage(
                                   rightMappedImageTexture.Texture,
                                   rightMappedImageTexture.SourceRect,
                                   rightRect);
                            });

                        _cache.Add(cacheKey, result);
                    }

                    return result;
                });
            }
            else
            {
                return null;
            }
        }

        private MappedImageTexture GetMappedImage(string mappedImageName)
        {
            if (string.IsNullOrEmpty(mappedImageName) || mappedImageName == "NoImage")
            {
                return null;
            }

            return _mappedImageManager.GetMappedImage(mappedImageName);
        }

        private Texture CreateTexture(
            in WndImageKey imageKey,
            Action<DrawingContext2D> drawCallback)
        {
            var imageTexture = AddDisposable(_contentManager.GraphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    (uint) imageKey.DestinationSize.Width,
                    (uint) imageKey.DestinationSize.Height,
                    1,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled | TextureUsage.RenderTarget)));

            imageTexture.Name = "WndImage";

            using (var drawingContext = new DrawingContext2D(_contentManager, imageTexture))
            {
                drawingContext.Begin(
                    _contentManager.PointClampSampler,
                    ColorRgbaF.Transparent);

                drawCallback(drawingContext);

                drawingContext.End();
            }

            return imageTexture;
        }
    }
}
