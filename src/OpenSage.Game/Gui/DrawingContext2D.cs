using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;
using OpenSage.Utilities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenSage.Gui
{
    public sealed class DrawingContext2D : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly RenderTarget _renderTarget;
        private readonly Texture _solidWhiteTexture;

        private readonly SpriteBatch _spriteBatch;

        private readonly ResourcePool<Image<Bgra32>, ImageKey> _textImagePool;
        private readonly ResourcePool<Texture, ImageKey> _textTexturePool;

        // TODO: Remove this.
        private readonly List<Texture> _texturesToDispose;

        private struct ImageKey
        {
            public int Width;
            public int Height;
        }

        private CommandBuffer _commandBuffer;
        private CommandEncoder _commandEncoder;

        public DrawingContext2D(
            ContentManager contentManager,
            Texture targetTexture)
        {
            _graphicsDevice = contentManager.GraphicsDevice;

            _renderTarget = AddDisposable(new RenderTarget(
                contentManager.GraphicsDevice,
                targetTexture));

            _solidWhiteTexture = AddDisposable(Texture.CreateTexture2D(
                contentManager.GraphicsDevice,
                PixelFormat.Rgba8UNorm,
                1,
                1,
                new[]
                {
                    new TextureMipMapData
                    {
                        BytesPerRow = 4,
                        Data = new byte[] { 255, 255, 255, 255 }
                    }
                }));

            _spriteBatch = AddDisposable(new SpriteBatch(contentManager));

            _textImagePool = AddDisposable(new ResourcePool<Image<Bgra32>, ImageKey>(key =>
                new Image<Bgra32>(key.Width, key.Height)));

            _textTexturePool = AddDisposable(new ResourcePool<Texture, ImageKey>(key => Texture.CreateTexture2D(
                contentManager.GraphicsDevice,
                PixelFormat.Bgra8UNorm,
                key.Width,
                key.Height,
                TextureBindFlags.ShaderResource)));

            _texturesToDispose = new List<Texture>();
        }

        public void Begin(
            SamplerState samplerState,
            in ColorRgbaF clearColor)
        {
            _commandBuffer = _graphicsDevice.CommandQueue.GetCommandBuffer();

            var renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                _renderTarget,
                LoadAction.Clear,
                clearColor);

            _commandEncoder = _commandBuffer.GetCommandEncoder(renderPassDescriptor);

            var viewport = new Viewport(
                0, 0,
                _renderTarget.Texture.Width,
                _renderTarget.Texture.Height);

            _commandEncoder.SetViewport(viewport);

            _spriteBatch.Begin(_commandEncoder, samplerState, viewport);
        }

        public void DrawImage(Texture texture, in Rectangle sourceRect, in Rectangle destinationRect)
        {
            _spriteBatch.DrawImage(texture, sourceRect, destinationRect.ToRectangleF(), ColorRgbaF.White);
        }

        public void DrawText(string text, Font font, TextAlignment textAlignment, ColorRgbaF color, RectangleF rect)
        {
            var image = _textImagePool.Acquire(new ImageKey
            {
                Width = (int) Math.Ceiling(rect.Width),
                Height = (int) Math.Ceiling(rect.Height)
            });

            image.Mutate(x =>
            {
                x.BackgroundColor(new Bgra32(0, 0, 0, 0));

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
            // TODO: This is terribly inefficient. Don't create texture every time.
            // Should use DangerousGetPinnableReferenceToPixelBuffer
            var pixelData = image.SavePixelData();

            var texture = Texture.CreateTexture2D(
                _graphicsDevice,
                PixelFormat.Bgra8UNorm,
                image.Width,
                image.Height,
                new[]
                {
                    new TextureMipMapData
                    {
                        BytesPerRow = image.Width * 4,
                        Data = pixelData
                    }
                });
            _texturesToDispose.Add(texture);

            _spriteBatch.DrawImage(
                texture,
                null,
                rect,
                ColorRgbaF.White);
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

            foreach (var texture in _texturesToDispose)
            {
                texture.Dispose();
            }
            _texturesToDispose.Clear();

            _textTexturePool.ReleaseAll();
            _textImagePool.ReleaseAll();

            _commandEncoder.Close();
            _commandBuffer.Commit();
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
