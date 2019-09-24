using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.Wnd;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd.Images
{
    public sealed class WndImageLoader : DisposableBase
    {
        private struct WndImageKey
        {
            public Size DestinationSize;
            public string LeftImage;
            public string MiddleImage;
            public string RightImage;
        }

        private readonly ContentManager _contentManager;
        private readonly AssetStore _assetStore;
        private readonly Dictionary<WndImageKey, Texture> _cache;

        internal WndImageLoader(ContentManager contentManager, AssetStore assetStore)
        {
            _contentManager = contentManager;
            _assetStore = assetStore;
            _cache = new Dictionary<WndImageKey, Texture>();
        }

        public Image CreateNormalImage(
            WndDrawData wndDrawData,
            int index)
        {
            return CreateNormalImage(wndDrawData.Items[index].Image);
        }

        public Image CreateFileImage(string fileImageName)
        {
            var requiresFlip = !_contentManager.GraphicsDevice.IsUvOriginTopLeft;
            var texture = _assetStore.GuiTextures.GetByName(fileImageName).Texture;

            return new Image(fileImageName, new Size((int)texture.Width, (int) texture.Height), size =>
            {
                var cacheKey = new WndImageKey
                {
                    DestinationSize = size,
                    LeftImage = fileImageName,
                };

                if (!_cache.TryGetValue(cacheKey, out var result))
                {
                    result = texture;

                    _cache.Add(cacheKey, result);
                }

                return result;
            }, requiresFlip);
        }

        public Image CreateNormalImage(string mappedImageName)
        {
            var leftImage = mappedImageName;
            var mappedImageTexture = GetMappedImage(leftImage);

            if (mappedImageTexture != null)
            {
                bool requiresFlip = !_contentManager.GraphicsDevice.IsUvOriginTopLeft;

                return new Image(mappedImageName, mappedImageTexture.Coords.Size, size =>
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
                            spriteBatch =>
                            {
                                spriteBatch.DrawImage(
                                    mappedImageTexture.Texture.Value,
                                    mappedImageTexture.Coords,
                                    new Rectangle(Point2D.Zero, size).ToRectangleF(),
                                    ColorRgbaF.White);
                            });

                        _cache.Add(cacheKey, result);
                    }

                    return result;
                }, requiresFlip);
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
                    leftMappedImageTexture.Coords.Width + middleMappedImageTexture.Coords.Width + rightMappedImageTexture.Coords.Width,
                    leftMappedImageTexture.Coords.Height);

                bool requiresFlip = !_contentManager.GraphicsDevice.IsUvOriginTopLeft;

                return new Image("WndImage", naturalSize, size =>
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
                            spriteBatch =>
                            {
                                var leftWidth = leftMappedImageTexture.Coords.Width;
                                var rightWidth = rightMappedImageTexture.Coords.Width;
                                var leftRect = new Rectangle(0, 0, leftWidth, cacheKey.DestinationSize.Height);
                                spriteBatch.DrawImage(
                                   leftMappedImageTexture.Texture.Value,
                                   leftMappedImageTexture.Coords,
                                   leftRect.ToRectangleF(),
                                   ColorRgbaF.White,
                                   requiresFlip);
                                var middleRect = new Rectangle(leftRect.Right, 0, cacheKey.DestinationSize.Width - leftWidth - rightWidth, cacheKey.DestinationSize.Height);
                                spriteBatch.DrawImage(
                                   middleMappedImageTexture.Texture.Value,
                                   middleMappedImageTexture.Coords,
                                   middleRect.ToRectangleF(),
                                   ColorRgbaF.White,
                                   requiresFlip);
                                var rightRect = new Rectangle(middleRect.Right, 0, rightWidth, cacheKey.DestinationSize.Height);
                                spriteBatch.DrawImage(
                                   rightMappedImageTexture.Texture.Value,
                                   rightMappedImageTexture.Coords,
                                   rightRect.ToRectangleF(),
                                   ColorRgbaF.White,
                                   requiresFlip);
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

        private MappedImage GetMappedImage(string mappedImageName)
        {
            if (string.IsNullOrEmpty(mappedImageName) || mappedImageName == "NoImage")
            {
                return null;
            }

            return _assetStore.MappedImages.GetByName(mappedImageName);
        }

        private Texture CreateTexture(
            in WndImageKey imageKey,
            Action<SpriteBatch> drawCallback)
        {
            var graphicsDevice = _contentManager.GraphicsDevice;

            var imageTexture = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    (uint) imageKey.DestinationSize.Width,
                    (uint) imageKey.DestinationSize.Height,
                    1,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled | TextureUsage.RenderTarget)));

            imageTexture.Name = "WndImage";

            var framebuffer = graphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, imageTexture));
            var spriteBatch = new SpriteBatch(
                new GraphicsLoadContext(graphicsDevice, _assetStore.LoadContext.StandardGraphicsResources, _assetStore.LoadContext.ShaderResources),
                BlendStateDescription.SingleDisabled,
                framebuffer.OutputDescription);
            var commandList = graphicsDevice.ResourceFactory.CreateCommandList();

            commandList.Begin();

            commandList.SetFramebuffer(framebuffer);

            spriteBatch.Begin(
                commandList,
                _assetStore.LoadContext.StandardGraphicsResources.PointClampSampler,
                new SizeF(imageTexture.Width, imageTexture.Height));

            spriteBatch.DrawImage(
                _assetStore.LoadContext.StandardGraphicsResources.SolidWhiteTexture,
                null,
                new RectangleF(0, 0, imageTexture.Width, imageTexture.Height),
                ColorRgbaF.Transparent);

            drawCallback(spriteBatch);

            spriteBatch.End();

            commandList.End();

            graphicsDevice.SubmitCommands(commandList);

            graphicsDevice.DisposeWhenIdle(commandList);
            graphicsDevice.DisposeWhenIdle(spriteBatch);
            graphicsDevice.DisposeWhenIdle(framebuffer);

            graphicsDevice.WaitForIdle();

            return imageTexture;
        }
    }
}
