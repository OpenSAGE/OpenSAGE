using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using SixLabors.Fonts;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui
{
    public sealed class DrawingContext2D : DisposableBase
    {
        private readonly ContentManager _contentManager;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Texture _solidWhiteTexture;

        private readonly SpriteBatch _spriteBatch;

        private readonly TextCache _textCache;

        private readonly Stack<Matrix3x2> _transformStack;
        private Matrix3x2 _currentTransform;
        private float _currentScale;

        private readonly Stack<float> _opacityStack;
        private float _currentOpacity;

        public DrawingContext2D(
            ContentManager contentManager,
            in BlendStateDescription blendStateDescription,
            in OutputDescription outputDescription)
        {
            _contentManager = contentManager;
            _graphicsDevice = contentManager.GraphicsDevice;

            _solidWhiteTexture = contentManager.SolidWhiteTexture;

            _spriteBatch = AddDisposable(new SpriteBatch(contentManager, blendStateDescription, outputDescription));

            _textCache = AddDisposable(new TextCache(contentManager));

            _transformStack = new Stack<Matrix3x2>();
            PushTransform(Matrix3x2.Identity);

            _opacityStack = new Stack<float>();
            PushOpacity(1);
        }

        public void Begin(
            CommandList commandList,
            Sampler samplerState,
            in SizeF outputSize)
        {
            _spriteBatch.Begin(
                commandList,
                samplerState,
                outputSize);
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

        public void DrawImage(
            Texture texture,
            in Rectangle? sourceRect,
            in Rectangle destinationRect,
            in bool flipped = false)
        {
            var color = ColorRgbaF.White.WithA(_currentOpacity);

            _spriteBatch.DrawImage(
                texture,
                sourceRect,
                RectangleF.Transform(destinationRect.ToRectangleF(), _currentTransform),
                color,
                flipped);
        }

        public void DrawImage(
            Texture texture,
            in Rectangle? sourceRect,
            in RectangleF destinationRect,
            in bool flipped = false)
        {
            var color = ColorRgbaF.White.WithA(_currentOpacity);

            _spriteBatch.DrawImage(
                texture,
                sourceRect,
                RectangleF.Transform(destinationRect, _currentTransform),
                color,
                flipped);
        }

        public static SizeF MeasureText(string text, Font font, TextAlignment textAlignment, float width)
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

        public unsafe void DrawText(string text, Font font, TextAlignment textAlignment, in ColorRgbaF color, in Rectangle rect)
        {
            DrawText(text, font, textAlignment, color, rect.ToRectangleF());
        }

        public unsafe void DrawText(string text, Font font, TextAlignment textAlignment, in ColorRgbaF color, in RectangleF rect)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var actualFont = _contentManager.GetOrCreateFont(font.Name, font.Size * _currentScale, font.Bold ? FontWeight.Bold : FontWeight.Normal);
            var actualRect = RectangleF.Transform(rect, _currentTransform);

            var actualColor = GetModifiedColorWithCurrentOpacity(color);

            var texture = _textCache.GetTextTexture(text, actualFont, textAlignment, actualColor, actualRect.Size);

            _spriteBatch.DrawImage(
                texture,
                null,
                actualRect,
                ColorRgbaF.White);
        }

        public void DrawRectangle(RectangleF rect, in ColorRgbaF strokeColor, float strokeWidth)
        {
            rect = RectangleF.Transform(rect, _currentTransform);
            strokeWidth *= _currentScale;

            var modifiedStrokeColor = GetModifiedColorWithCurrentOpacity(strokeColor);

            void drawLine(RectangleF lineRect)
            {
                _spriteBatch.DrawImage(
                    _solidWhiteTexture,
                    null,
                    lineRect,
                    modifiedStrokeColor);
            }

            drawLine(new RectangleF(rect.X, rect.Y, strokeWidth, rect.Height)); // Left
            drawLine(new RectangleF(rect.X, rect.Y, rect.Width, strokeWidth));  // Top
            drawLine(new RectangleF(rect.X, rect.Bottom - strokeWidth, rect.Width, strokeWidth)); // Bottom
            drawLine(new RectangleF(rect.Right - strokeWidth, rect.Y, strokeWidth, rect.Height)); // Right
        }

        private ColorRgbaF GetModifiedColorWithCurrentOpacity(in ColorRgbaF color)
        {
            return color.WithA(color.A * _currentOpacity);
        }

        public void DrawLine(Line2D line, float thickness, in ColorRgbaF strokeColor)
        {
            line = Line2D.Transform(line, _currentTransform);
            thickness *= _currentScale;

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
                GetModifiedColorWithCurrentOpacity(strokeColor));
        }

        public void FillTriangle(in Triangle2D triangle, in ColorRgbaF fillColor)
        {
            var modifiedFillColor = fillColor.WithA(fillColor.A * _currentOpacity);

            _spriteBatch.DrawImage(
                _solidWhiteTexture,
                new Triangle2D(new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0)),
                Triangle2D.Transform(triangle, _currentTransform),
                modifiedFillColor);
        }

        public void FillTriangle(Texture texture, in Triangle2D sourceTriangle, in Triangle2D triangle, in ColorRgbaF tintColor)
        {
            _spriteBatch.DrawImage(
                texture,
                sourceTriangle,
                Triangle2D.Transform(triangle, _currentTransform),
                GetModifiedColorWithCurrentOpacity(tintColor));
        }

        public void FillRectangle(in Rectangle rect, in ColorRgbaF fillColor)
        {
            _spriteBatch.DrawImage(
                _solidWhiteTexture,
                new Rectangle(0, 0, 1, 1),
                RectangleF.Transform(rect.ToRectangleF(), _currentTransform),
                GetModifiedColorWithCurrentOpacity(fillColor));
        }

        public void End()
        {
            _spriteBatch.End();
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
