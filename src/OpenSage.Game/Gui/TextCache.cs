using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenSage.Mathematics;
using OpenSage.Utilities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;

namespace OpenSage.Gui
{
    internal sealed class TextCache : DisposableBase
    {
        private readonly struct TextKey : IEquatable<TextKey>
        {
            public readonly string Text;
            public readonly Font Font;
            public readonly TextAlignment Alignment;
            public readonly ColorRgbaF Color;
            public readonly SizeF Size;

            public TextKey(string text, Font font, TextAlignment alignment, in ColorRgbaF color, in SizeF size)
            {
                Text = text;
                Font = font;
                Alignment = alignment;
                Color = color;
                Size = size;
            }

            public override bool Equals(object obj)
            {
                return obj is TextKey && Equals((TextKey) obj);
            }

            public bool Equals(TextKey other)
            {
                return
                    Text == other.Text &&
                    Font == other.Font &&
                    Alignment == other.Alignment &&
                    Color == other.Color &&
                    Size == other.Size;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Text, Font, Alignment, Color, Size);
            }

            public static bool operator ==(TextKey key1, TextKey key2)
            {
                return key1.Equals(key2);
            }

            public static bool operator !=(TextKey key1, TextKey key2)
            {
                return !(key1 == key2);
            }
        }

        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<TextKey, Texture> _cache;

        private struct ImageKey
        {
            public int Width;
            public int Height;
        }

        private readonly ResourcePool<Image<Bgra32>, ImageKey> _textImagePool;

        public TextCache(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            _cache = new Dictionary<TextKey, Texture>();

            _textImagePool = AddDisposable(new ResourcePool<Image<Bgra32>, ImageKey>(key =>
                new Image<Bgra32>(key.Width, key.Height)));
        }

        public Texture GetTextTexture(
            string text,
            Font font,
            TextAlignment textAlignment,
            in ColorRgbaF color,
            in SizeF size)
        {
            var key = new TextKey(text, font, textAlignment, color, size);

            if (!_cache.TryGetValue(key, out var result))
            {
                _cache.Add(key, result = AddDisposable(CreateTexture(key)));
            }

            return result;
        }

        private unsafe Texture CreateTexture(TextKey key)
        {
            var size = key.Size;
            var actualFont = key.Font;

            var image = _textImagePool.Acquire(new ImageKey
            {
                Width = (int) MathF.Ceiling(size.Width),
                Height = (int) MathF.Ceiling(size.Height)
            });

            // Clear image to transparent.
            // TODO: Don't need to do this for a newly created image.
            image.GetPixelSpan().Clear();

            image.Mutate(x =>
            {
                var location = new SixLabors.Primitives.PointF(0, size.Height / 2.0f);

                // TODO: Vertical centering is not working properly.
                location.Y *= 0.8f;

                var color = key.Color;

                try
                {
                    x.DrawText(
                        new TextGraphicsOptions
                        {
                            WrapTextWidth = size.Width,
                            HorizontalAlignment = key.Alignment == TextAlignment.Center
                                ? HorizontalAlignment.Center
                                : HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        key.Text,
                        actualFont,
                        new Bgra32(
                            (byte) (color.R * 255.0f),
                            (byte) (color.G * 255.0f),
                            (byte) (color.B * 255.0f),
                            (byte) (color.A * 255.0f)),
                        location);
                }
                catch
                {
                    // TODO:
                }
            });

            Texture texture;

            // Draw image to texture.
            fixed (void* pin = &MemoryMarshal.GetReference(image.GetPixelSpan()))
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
