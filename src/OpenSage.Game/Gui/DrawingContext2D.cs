using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using OpenSage.Utilities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui
{
    public sealed class DrawingContext2D : DisposableBase
    {
        private readonly ContentManager _contentManager;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Framebuffer _renderTarget;
        private readonly Texture _solidWhiteTexture;

        private readonly SpriteBatch _spriteBatch;

        private readonly TextCache _textCache;

        private readonly Stack<Matrix3x2> _transformStack;
        private Matrix3x2 _currentTransform;
        private float _currentScale;

        private readonly Stack<float> _opacityStack;
        private float _currentOpacity;

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
            _contentManager = contentManager;
            _graphicsDevice = contentManager.GraphicsDevice;

            _renderTarget = AddDisposable(_graphicsDevice.ResourceFactory.CreateFramebuffer(
                new FramebufferDescription(null, targetTexture)));

            _solidWhiteTexture = contentManager.SolidWhiteTexture;

            _spriteBatch = AddDisposable(new SpriteBatch(contentManager, _renderTarget.OutputDescription));

            _textCache = AddDisposable(new TextCache(contentManager));

            _commandEncoder = AddDisposable(_graphicsDevice.ResourceFactory.CreateCommandList());

            _transformStack = new Stack<Matrix3x2>();
            _transformStack.Push(Matrix3x2.Identity);

            _opacityStack = new Stack<float>();
            _opacityStack.Push(1);
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
                viewport);
        }

        public void PushTransform(in Matrix3x2 transform)
        {
            _transformStack.Push(transform);
            UpdateTransform();
        }

        public void PopTransform()
        {
            _transformStack.Pop();
            UpdateTransform();
        }

        private void UpdateTransform()
        {
            _currentTransform = Matrix3x2.Identity;
            foreach (var matrix in _transformStack)
            {
                _currentTransform *= matrix;
            }

            // Only works for uniform scale.
            _currentScale = MathUtility.Sqrt(_currentTransform.M11 * _currentTransform.M11 + _currentTransform.M21 * _currentTransform.M21);
        }

        public void PushOpacity(float opacity)
        {
            _opacityStack.Push(opacity);
            UpdateOpacity();
        }

        public void PopOpacity()
        {
            _opacityStack.Pop();
            UpdateOpacity();
        }

        private void UpdateOpacity()
        {
            _currentOpacity = 1;
            foreach (var opacity in _opacityStack)
            {
                _currentOpacity *= opacity;
            }
        }

        public void DrawImage(Texture texture, in Rectangle? sourceRect, in Rectangle destinationRect)
        {
            var color = ColorRgbaF.White;
            color.A = _currentOpacity;

            _spriteBatch.DrawImage(
                texture,
                sourceRect,
                RectangleF.Transform(destinationRect.ToRectangleF(), _currentTransform),
                color);
        }

        public void DrawImage(Texture texture, in Rectangle? sourceRect, in RectangleF destinationRect)
        {
            var color = ColorRgbaF.White;
            color.A = _currentOpacity;

            _spriteBatch.DrawImage(
                texture,
                sourceRect,
                RectangleF.Transform(destinationRect, _currentTransform),
                color);
        }

        public SizeF MeasureText(string text, Font font, TextAlignment textAlignment, float width)
        {
            var textSize = TextMeasurer.Measure(
                text,
                new RendererOptions(font)
                {
                    WrappingWidth = width,
                    HorizontalAlignment = textAlignment == TextAlignment.Center
                        ? HorizontalAlignment.Center
                        : HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                });

            return new SizeF(textSize.Width, textSize.Height);
        }

        public unsafe void DrawText(string text, in DrawingFont font, TextAlignment textAlignment, in ColorRgbaF color, in Rectangle rect)
        {
            DrawText(text, font, textAlignment, color, rect.ToRectangleF());
        }

        public unsafe void DrawText(string text, in DrawingFont font, TextAlignment textAlignment, in ColorRgbaF color, in RectangleF rect)
        {
            var actualFont = new DrawingFont(font.Name, font.Size * _currentScale, font.Bold);
            var actualRect = RectangleF.Transform(rect, _currentTransform);

            var actualColor = color;
            actualColor.A *= _currentOpacity;

            var texture = _textCache.GetTextTexture(text, actualFont, textAlignment, actualColor, actualRect.Size);

            _spriteBatch.DrawImage(
                texture,
                null,
                actualRect,
                ColorRgbaF.White);
        }

        public void DrawRectangle(RectangleF rect, ColorRgbaF strokeColor, float strokeWidth)
        {
            rect = RectangleF.Transform(rect, _currentTransform);
            strokeWidth *= _currentScale;

            strokeColor.A *= _currentOpacity;

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

        public void DrawLine(Line2D line, float thickness, ColorRgbaF strokeColor)
        {
            line = Line2D.Transform(line, _currentTransform);
            thickness *= _currentScale;

            var length = Vector2.Distance(line.V0, line.V1);
            var angle = MathUtility.Atan2(line.V1.Y - line.V0.Y, line.V1.X - line.V0.X);

            var origin = new Vector2(0, 0.5f);
            var scale = new Vector2(length, thickness);

            strokeColor.A *= _currentOpacity;

            _spriteBatch.DrawImage(
                _solidWhiteTexture,
                null,
                line.V0,
                angle,
                origin,
                scale,
                strokeColor);
        }

        public void FillTriangle(in Triangle2D triangle, ColorRgbaF fillColor)
        {
            fillColor.A *= _currentOpacity;

            _spriteBatch.DrawImage(
                _solidWhiteTexture,
                new Triangle2D(new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0)),
                Triangle2D.Transform(triangle, _currentTransform),
                fillColor);
        }

        public void FillTriangle(Texture texture, in Triangle2D sourceTriangle, in Triangle2D triangle, ColorRgbaF tintColor)
        {
            tintColor.A *= _currentOpacity;

            _spriteBatch.DrawImage(
                texture,
                sourceTriangle,
                Triangle2D.Transform(triangle, _currentTransform),
                tintColor);
        }

        public void FillRectangle(in Rectangle rect, ColorRgbaF fillColor)
        {
            fillColor.A *= _currentOpacity;

            _spriteBatch.DrawImage(
                _solidWhiteTexture,
                new Rectangle(0, 0, 1, 1),
                RectangleF.Transform(rect.ToRectangleF(), _currentTransform),
                fillColor);
        }

        public void End()
        {
            _spriteBatch.End();

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

    public readonly struct DrawingFont
    {
        public readonly string Name;
        public readonly float Size;
        public readonly bool Bold;

        public DrawingFont(string name, float size, bool bold)
        {
            Name = name;
            Size = size;
            Bold = bold;
        }
    }
}
