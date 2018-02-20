using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Content.Util;
using OpenSage.Data.Wnd;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd
{
    internal sealed class WndImageTextureCache : DisposableBase
    {
        private struct WndImageKey
        {
            public Size DestinationSize;
            public string LeftImage;
            public string MiddleImage;
            public string RightImage;
        }

        private readonly ContentManager _contentManager;
        private readonly MappedImageManager _mappedImageManager;
        private readonly Dictionary<WndImageKey, Texture> _cache;

        public WndImageTextureCache(ContentManager contentManager, MappedImageManager mappedImageManager)
        {
            _contentManager = contentManager;
            _mappedImageManager = mappedImageManager;
            _cache = new Dictionary<WndImageKey, Texture>();
        }

        public Texture GetNormalTexture(
            WndWindowDefinition wndWindow,
            WndDrawData wndDrawData,
            int index)
        {
            var cacheKey = new WndImageKey
            {
                DestinationSize = wndWindow.ScreenRect.ToRectangle().Size,
                LeftImage = wndDrawData.Items[index].Image
            };

            if (!_cache.TryGetValue(cacheKey, out var result))
            {
                var mappedImageTexture = GetMappedImage(cacheKey.LeftImage);

                if (mappedImageTexture != null)
                {
                    result = CreateTexture(
                        cacheKey,
                        drawingContext =>
                        {
                            drawingContext.DrawImage(
                                mappedImageTexture.Texture,
                                mappedImageTexture.SourceRect,
                                new Rectangle(Point2D.Zero, cacheKey.DestinationSize));
                        });
                }
                else
                {
                    result = null;
                }

                _cache.Add(cacheKey, result);
            }

            return result;
        }

        public Texture GetStretchableTexture(
            WndWindowDefinition wndWindow,
            WndDrawData wndDrawData,
            int leftIndex,
            int middleIndex,
            int rightIndex)
        {
            var cacheKey = new WndImageKey
            {
                DestinationSize = wndWindow.ScreenRect.ToRectangle().Size,
                LeftImage = wndDrawData.Items[leftIndex].Image,
                MiddleImage = wndDrawData.Items[middleIndex].Image,
                RightImage = wndDrawData.Items[rightIndex].Image
            };

            if (!_cache.TryGetValue(cacheKey, out var result))
            {
                var leftMappedImageTexture = GetMappedImage(cacheKey.LeftImage);
                var middleMappedImageTexture = GetMappedImage(cacheKey.MiddleImage);
                var rightMappedImageTexture = GetMappedImage(cacheKey.RightImage);

                if (leftMappedImageTexture != null &&
                    middleMappedImageTexture != null &&
                    rightMappedImageTexture != null)
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
                }
                else
                {
                    result = null;
                }

                _cache.Add(cacheKey, result);
            }

            return result;
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
