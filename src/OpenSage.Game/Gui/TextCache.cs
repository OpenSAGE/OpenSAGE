using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using OpenSage.Content;
using OpenSage.Mathematics;
using OpenSage.Utilities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace OpenSage.Gui
{
    internal sealed class TextCache : DisposableBase
    {
        private struct TextKey
        {
            public string Text;
            public DrawingFont Font;
            public TextAlignment Alignment;
            public ColorRgbaF Color;
            public SizeF Size;
        }

        private readonly ContentManager _contentManager;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<TextKey, Texture> _cache;

        private struct ImageKey
        {
            public int Width;
            public int Height;
        }

        private readonly ResourcePool<Image<Bgra32>, ImageKey> _textImagePool;

        public TextCache(ContentManager contentManager)
        {
            _contentManager = contentManager;
            _graphicsDevice = contentManager.GraphicsDevice;

            _cache = new Dictionary<TextKey, Texture>();

            _textImagePool = AddDisposable(new ResourcePool<Image<Bgra32>, ImageKey>(key =>
                new Image<Bgra32>(key.Width, key.Height)));
        }

        public Texture GetTextTexture(string text, in DrawingFont font, TextAlignment textAlignment, ColorRgbaF color, SizeF size)
        {
            var key = new TextKey
            {
                Text = text,
                Font = font,
                Alignment = textAlignment,
                Color = color,
                Size = size
            };

            if (!_cache.TryGetValue(key, out var result))
            {
                _cache.Add(key, result = AddDisposable(CreateTexture(key)));
            }

            return result;
        }

        private unsafe Texture CreateTexture(TextKey key)
        {
            var size = key.Size;

            var actualFont = _contentManager.GetOrCreateFont(
                key.Font.Name,
                key.Font.Size,
                key.Font.Bold ? FontWeight.Bold : FontWeight.Normal);

            var image = _textImagePool.Acquire(new ImageKey
            {
                Width = (int) Math.Ceiling(size.Width),
                Height = (int) Math.Ceiling(size.Height)
            });

            // Clear image to transparent.
            // TODO: Don't need to do this for a newly created image.
            fixed (void* pin = &image.DangerousGetPinnableReferenceToPixelBuffer())
            {
                Unsafe.InitBlock(pin, 0, (uint) (image.Width * image.Height * 4));
            }

            image.Mutate(x =>
            {
                var location = new SixLabors.Primitives.PointF(0, size.Height / 2.0f);

                // TODO: Vertical centering is not working properly.
                location.Y *= 0.8f;

                var color = key.Color;

                x.DrawText(
                    key.Text,
                    actualFont,
                    new Bgra32(
                        (byte) (color.R * 255.0f),
                        (byte) (color.G * 255.0f),
                        (byte) (color.B * 255.0f),
                        (byte) (color.A * 255.0f)),
                    location,
                    new TextGraphicsOptions
                    {
                        WrapTextWidth = size.Width,
                        HorizontalAlignment = key.Alignment == TextAlignment.Center
                            ? HorizontalAlignment.Center
                            : HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center
                    });
            });

            Texture texture;

            // Draw image to texture.
            fixed (void* pin = &image.DangerousGetPinnableReferenceToPixelBuffer())
            {
                texture = _graphicsDevice.ResourceFactory.CreateTexture(
                    TextureDescription.Texture2D(
                        (uint) image.Width,
                        (uint) image.Height,
                        1,
                        1,
                        PixelFormat.B8_G8_R8_A8_UNorm,
                        TextureUsage.Sampled));

                _graphicsDevice.UpdateTexture(
                    texture,
                    new IntPtr(pin),
                    (uint) (image.Width * image.Height * 4),
                    0, 0, 0,
                    texture.Width,
                    texture.Height,
                    1,
                    0,
                    0);
            }

            _textImagePool.ReleaseAll();

            return texture;
        }

        // TODO: Remove textures that haven't been used for a few frames.
    }
}
