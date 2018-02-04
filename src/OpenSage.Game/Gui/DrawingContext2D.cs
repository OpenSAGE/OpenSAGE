using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace OpenSage.Gui
{
    public sealed class DrawingContext2D : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Framebuffer _renderTarget;
        private readonly Texture _solidWhiteTexture;

        private readonly SpriteBatch _spriteBatch;

        private readonly ResourcePool<Image<Bgra32>, ImageKey> _textImagePool;
        private readonly ResourcePool<Texture, ImageKey> _textTexturePool;

        private struct ImageKey
        {
            public int Width;
            public int Height;
        }

        private CommandList _commandEncoder;

        public DrawingContext2D(
            ContentManager contentManager,
            Texture targetTexture)
        {
            _graphicsDevice = contentManager.GraphicsDevice;

            _renderTarget = AddDisposable(_graphicsDevice.ResourceFactory.CreateFramebuffer(
                new FramebufferDescription(null, targetTexture)));

            _solidWhiteTexture = AddDisposable(_graphicsDevice.CreateStaticTexture2D(
                1,
                1,
                new TextureMipMapData(
                    new byte[] { 255, 255, 255, 255 },
                    4, 4, 1, 1),
                PixelFormat.R8_G8_B8_A8_UNorm));

            _spriteBatch = AddDisposable(new SpriteBatch(contentManager));

            _textImagePool = AddDisposable(new ResourcePool<Image<Bgra32>, ImageKey>(key =>
                new Image<Bgra32>(key.Width, key.Height)));

            _textTexturePool = AddDisposable(new ResourcePool<Texture, ImageKey>(key =>
                _graphicsDevice.ResourceFactory.CreateTexture(
                    TextureDescription.Texture2D(
                        (uint) key.Width,
                        (uint) key.Height,
                        1,
                        1,
                        PixelFormat.B8_G8_R8_A8_UNorm,
                        TextureUsage.Sampled))));

            _commandEncoder = AddDisposable(_graphicsDevice.ResourceFactory.CreateCommandList());
        }

        public void Begin(
            Sampler samplerState,
            in ColorRgbaF clearColor)
        {
            _commandEncoder.Begin();

            _commandEncoder.SetFramebuffer(_renderTarget);

            _commandEncoder.ClearColorTarget(0, clearColor.ToRgbaFloat());

            var viewport = new Viewport(
                0, 0,
                _renderTarget.Width,
                _renderTarget.Height,
                0,
                1);

            _commandEncoder.SetViewport(0, ref viewport);

            _spriteBatch.Begin(
                _commandEncoder,
                samplerState,
                _renderTarget.OutputDescription,
                viewport);
        }

        public void DrawImage(Texture texture, in Rectangle sourceRect, in Rectangle destinationRect)
        {
            _spriteBatch.DrawImage(texture, sourceRect, destinationRect.ToRectangleF(), ColorRgbaF.White);
        }

        public unsafe void DrawText(string text, Font font, TextAlignment textAlignment, ColorRgbaF color, RectangleF rect)
        {
            var image = _textImagePool.Acquire(new ImageKey
            {
                Width = (int) Math.Ceiling(rect.Width),
                Height = (int) Math.Ceiling(rect.Height)
            });

            image.Mutate(x =>
            {
                x.Fill(new Bgra32(0, 0, 0, 0));

                var location = new SixLabors.Primitives.PointF(0, rect.Height / 2.0f);

                // TODO: Vertical centering is not working properly.
                location.Y *= 0.8f;

                x.DrawText(
                    text,
                    font,
                    new Bgra32(
                        (byte) (color.R * 255.0f),
                        (byte) (color.G * 255.0f),
                        (byte) (color.B * 255.0f),
                        (byte) (color.A * 255.0f)),
                    location,
                    new TextGraphicsOptions
                    {
                        WrapTextWidth = rect.Width,
                        HorizontalAlignment = textAlignment == TextAlignment.Center
                            ? HorizontalAlignment.Center
                            : HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center
                    });
            });

            // Draw image to texture.
            fixed (void* pin = &image.DangerousGetPinnableReferenceToPixelBuffer())
            {
                var texture = _textTexturePool.Acquire(new ImageKey
                {
                    Width = image.Width,
                    Height = image.Height
                });

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

                _spriteBatch.DrawImage(
                    texture,
                    null,
                    rect,
                    ColorRgbaF.White);
            }
        }

        public void DrawRectangle(in RectangleF rect, ColorRgbaF strokeColor, float strokeWidth)
        {
            void drawLine(RectangleF lineRect)
            {
                _spriteBatch.DrawImage(
                    _solidWhiteTexture,
                    null,
                    lineRect,
                    strokeColor);
            }

            drawLine(new RectangleF(rect.X, rect.Y, strokeWidth, rect.Height)); // Left
            drawLine(new RectangleF(rect.X, rect.Y, rect.Width, strokeWidth));  // Top
            drawLine(new RectangleF(rect.X, rect.Bottom - strokeWidth, rect.Width, strokeWidth)); // Bottom
            drawLine(new RectangleF(rect.Right - strokeWidth, rect.Y, strokeWidth, rect.Height)); // Right
        }

        public void DrawLine(in Line2D line, float thickness, in ColorRgbaF strokeColor)
        {
            var length = Vector2.Distance(line.V0, line.V1);
            var angle = MathUtility.Atan2(line.V1.Y - line.V0.Y, line.V1.X - line.V0.X);

            var origin = new Vector2(0, 0.5f);
            var scale = new Vector2(length, thickness);

            _spriteBatch.DrawImage(
                _solidWhiteTexture,
                null,
                line.V0,
                angle,
                origin,
                scale,
                strokeColor);
        }

        public void FillTriangle(in Triangle2D triangle, in ColorRgbaF fillColor)
        {
            _spriteBatch.DrawImage(
                _solidWhiteTexture,
                new Triangle2D(new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0)),
                triangle,
                fillColor);
        }

        public void FillTriangle(Texture texture, in Triangle2D sourceTriangle, in Triangle2D triangle, in ColorRgbaF tintColor)
        {
            _spriteBatch.DrawImage(
                texture,
                sourceTriangle,
                triangle,
                tintColor);
        }

        public void FillRectangle(in Rectangle rect, in ColorRgbaF fillColor)
        {
            _spriteBatch.DrawImage(
                _solidWhiteTexture,
                new Rectangle(0, 0, 1, 1),
                rect.ToRectangleF(),
                fillColor);
        }

        public void End()
        {
            _spriteBatch.End();

            _textTexturePool.ReleaseAll();
            _textImagePool.ReleaseAll();

            _commandEncoder.End();

            _graphicsDevice.SubmitCommands(_commandEncoder);
        }
    }

    public enum TextAlignment
    {
        Leading,
        Center
    }

    public enum FontWeight
    {
        Normal,
        Bold
    }
}
