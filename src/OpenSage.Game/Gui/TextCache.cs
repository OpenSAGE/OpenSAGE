﻿using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Mathematics;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using SizeF = OpenSage.Mathematics.SizeF;

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
                return obj is TextKey key && Equals(key);
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

        private class TextCacheEntry : IDisposable
        {
            public readonly Texture Texture;
            public TimeSpan LastUsed { get; private set;}

            public TextCacheEntry(Texture texture, TimeSpan now)
            {
                Texture = texture;
                LastUsed = now;
            }

            public bool IsExpired(TimeSpan now)
            {
                // TODO: Make this configurable.
                return now - LastUsed > TimeSpan.FromSeconds(5);
            }

            public void MarkUsed(TimeSpan now)
            {
                LastUsed = now;
            }

            public void Dispose()
            {
                Texture.Dispose();
            }
        }

        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<TextKey, TextCacheEntry> _cache;

        private struct ImageKey
        {
            public int Width;
            public int Height;
        }

        private readonly ResourcePool<Image<Bgra32>, ImageKey> _textImagePool;

        public TextCache(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            _cache = new Dictionary<TextKey, TextCacheEntry>();

            _textImagePool = AddDisposable(new ResourcePool<Image<Bgra32>, ImageKey>(key =>
                new Image<Bgra32>(key.Width, key.Height)));
        }

        public Texture GetTextTexture(
            string text,
            Font font,
            TextAlignment textAlignment,
            in ColorRgbaF color,
            in SizeF size,
            in TimeInterval now)
        {
            var key = new TextKey(text, font, textAlignment, color, size);

            if (_cache.TryGetValue(key, out var result))
            {
                result.MarkUsed(now.TotalTime);
                return result.Texture;
            }

            var texture = CreateTexture(key);
            var cacheEntry = AddDisposable(new TextCacheEntry(texture, now.TotalTime));
            _cache.Add(key, cacheEntry);
            return texture;
        }

        public void ClearExpiredEntries(in TimeInterval now)
        {
            var removedEntries = new List<TextKey>(0);

            foreach (var (key, value) in _cache)
            {
                if (value.IsExpired(now.TotalTime))
                {
                    // We need to copy the value into a local variable because
                    // we can't pass a foreach iteration variable as a ref
                    var cacheValue = value;
                    RemoveAndDispose(ref cacheValue);
                    removedEntries.Add(key);
                }
            }

            foreach (var key in removedEntries)
            {
                _cache.Remove(key);
            }
        }

        private unsafe Texture CreateTexture(TextKey key)
        {
            var size = key.Size;
            var actualFont = key.Font;

            var image = _textImagePool.Acquire(
                new ImageKey
                {
                    Width = (int) MathF.Ceiling(size.Width),
                    Height = (int) MathF.Ceiling(size.Height)
                },
                out var isNew
            );

            image.Mutate(x =>
            {
                var location = new PointF(0, size.Height / 2.0f);
                var color = key.Color;

                // Clear re-used image buffers
                if (!isNew)
                {
                    x.Clear(Color.Transparent);
                }

                try
                {
                    // '&' is used to bold the single character following it
                    var hotkeyIndex = key.Text.IndexOf('&');
                    var hotkeyFont = new Font(actualFont, FontStyle.Bold);
                    var text1 = hotkeyIndex < 0 ? key.Text : key.Text[..hotkeyIndex];

                    var drawOptions = new DrawingOptions
                    {
                        GraphicsOptions = new GraphicsOptions { Antialias = true, },
                    };

                    var wrappingLength = size.Width;
                    var horizontalAlignment = key.Alignment == TextAlignment.Center
                        ? HorizontalAlignment.Center
                        : HorizontalAlignment.Left;
                    var verticalAlignment = VerticalAlignment.Center;

                    if (horizontalAlignment == HorizontalAlignment.Center)
                    {
                        location.X = size.Width / 2f;
                    }

                    var regularRenderOptions = new RichTextOptions(actualFont)
                    {
                        WrappingLength = wrappingLength,
                        HorizontalAlignment = horizontalAlignment,
                        VerticalAlignment = verticalAlignment,
                        Origin = location,
                    };

                    var drawColor = new Bgra32(
                        (byte)(color.R * 255.0f),
                        (byte)(color.G * 255.0f),
                        (byte)(color.B * 255.0f),
                        (byte)(color.A * 255.0f));

                    var ctx = x.DrawText(
                        drawOptions,
                        regularRenderOptions,
                        text1,
                        new SolidBrush(drawColor),
                        null);

                    if (hotkeyIndex > -1)
                    {
                        var ogSize = TextMeasurer.MeasureBounds(text1, regularRenderOptions);

                        var boldRenderOptions = new RichTextOptions(hotkeyFont)
                        {
                            WrappingLength = wrappingLength,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = verticalAlignment,
                            Origin = location with { X = ogSize.Right },
                        };

                        var boldChar = key.Text[hotkeyIndex + 1].ToString();
                        var boldSize = TextMeasurer.MeasureBounds(boldChar, boldRenderOptions);

                        var remainingRenderOptions = new RichTextOptions(actualFont)
                        {
                            WrappingLength = wrappingLength,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = verticalAlignment,
                            Origin = new Vector2(boldSize.Right, regularRenderOptions.Origin.Y),
                        };

                        ctx.DrawText(
                                drawOptions,
                                boldRenderOptions,
                                boldChar,
                                new SolidBrush(
                                    new Bgra32(
                                        255, // hotkey text is yellow
                                        255,
                                        0,
                                        (byte)(color.A * 255.0f))),
                                null)
                            .DrawText(
                                drawOptions,
                                remainingRenderOptions,
                                key.Text[(hotkeyIndex + 2)..],
                                new SolidBrush(drawColor),
                                null);
                    }
                }
                catch
                {
                    // TODO:
                }
            });

            Texture texture;

            // Draw image to texture.
            if (!image.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> pixelSpan))
            {
                throw new InvalidOperationException("Unable to get image pixelspan.");
            }
            using (var pin = pixelSpan.Pin())
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
                    new IntPtr(pin.Pointer),
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

        internal void DrawDiagnostic(ref object selectedObject)
        {
            ImGui.BeginChild("text cache", Vector2.Zero, ImGuiChildFlags.Border);

            foreach (var ((key, entry), index) in _cache.WithIndex())
            {
                if (ImGui.Selectable($"{key.Text} {entry.Texture.Width}x{entry.Texture.Height}##{index}", selectedObject == entry.Texture)) {
                    selectedObject = entry.Texture;
                }
            }

            ImGui.EndChild();
        }
    }
}
