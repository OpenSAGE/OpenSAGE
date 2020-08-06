using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Graphics;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Apt
{
    public sealed class AptRenderingContext : DisposableBase
    {
        private readonly ContentManager _contentManager;
        private readonly AptContext _aptContext;
        private readonly GraphicsLoadContext _graphicsLoadContext;
        private readonly Stack<ItemTransform> _transformStack;

        private DrawingContext2D _drawingContext;

        private readonly DrawingContext2D _clipMaskDrawingContext;
        private readonly CommandList _commandList;

        private DrawingContext2D _activeDrawingContext;

        public AptWindow Window { get; }

        public Size WindowSize { get; private set; }

        public ItemTransform CurrentTransform => _transformStack.Peek();

        internal AptRenderingContext(
            AptWindow window,
            ContentManager contentManager,
            GraphicsLoadContext graphicsLoadContext,
            AptContext aptContext)
        {
            _contentManager = contentManager;
            _aptContext = aptContext;
            _graphicsLoadContext = graphicsLoadContext;

            Window = window;

            _transformStack = new Stack<ItemTransform>();

            _clipMaskDrawingContext = AddDisposable(
                new DrawingContext2D(
                    contentManager,
                    graphicsLoadContext,
                    BlendStateDescription.SingleAlphaBlend,
                    RenderTarget.OutputDescription));

            _commandList = AddDisposable(contentManager.GraphicsDevice.ResourceFactory.CreateCommandList());
        }

        private void CalculateTransform(ref ItemTransform transform)
        {
            if (Window == null)
            {
                return;
            }

            var scaling = Window.GetScaling();
            transform.GeometryRotation.M11 *= scaling.X;
            transform.GeometryRotation.M22 *= scaling.Y;
            transform.GeometryRotation.Translation = transform.GeometryTranslation * scaling;
        }

        internal void SetRenderTarget(RenderTarget renderTarget)
        {
            if (renderTarget != null)
            {
                _commandList.Begin();

                _commandList.SetFramebuffer(renderTarget.Framebuffer);
                _commandList.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 0));

                _clipMaskDrawingContext.Begin(
                    _commandList,
                    _graphicsLoadContext.StandardGraphicsResources.LinearClampSampler,
                    new SizeF(renderTarget.ColorTarget.Width, renderTarget.ColorTarget.Height));

                _activeDrawingContext = _clipMaskDrawingContext;
            }
            else
            {
                _clipMaskDrawingContext.End();

                _commandList.End();

                _contentManager.GraphicsDevice.SubmitCommands(_commandList);

                _activeDrawingContext = _drawingContext;
            }
        }

        public void SetDrawingContext(DrawingContext2D drawingContext)
        {
            _drawingContext = drawingContext;
            _activeDrawingContext = drawingContext;
        }

        public void PushTransform(in ItemTransform transform)
        {
            var newTransform = _transformStack.Count > 0
                ? _transformStack.Peek() * transform
                : transform;

            _transformStack.Push(newTransform);
        }

        public void PopTransform()
        {
            _transformStack.Pop();
        }

        // TODO
        internal void SetClipMask(RenderTarget renderTarget)
        {
            _drawingContext.SetAlphaMask(renderTarget?.ColorTarget);
        }

        public void RenderText(Text text)
        {
            var font = _contentManager.FontManager.GetOrCreateFont("Arial", text.FontHeight, FontWeight.Normal);

            var transform = _transformStack.Peek();
            CalculateTransform(ref transform);

            _drawingContext.DrawText(
                text.Content,
                font,
                TextAlignment.Center,
                text.Color.ToColorRgbaF() * transform.ColorTransform,
                RectangleF.Transform(text.Bounds, transform.GeometryRotation));
        }

        public void RenderGeometry(Geometry shape, Texture solidTexture)
        {
            var transform = _transformStack.Peek();
            CalculateTransform(ref transform);
            var matrix = transform.GeometryRotation;

            foreach (var e in shape.Entries)
            {
                switch (e)
                {
                    case GeometryLines l:
                        {
                            var color = l.Color.ToColorRgbaF() * transform.ColorTransform;
                            foreach (var line in l.Lines)
                            {
                                _activeDrawingContext.DrawLine(
                                    Line2D.Transform(line, matrix),
                                    l.Thickness,
                                    color);
                            }
                            break;
                        }

                    case GeometrySolidTriangles st:
                        {
                            var color = st.Color.ToColorRgbaF() * transform.ColorTransform;
                            foreach (var tri in st.Triangles)
                            {
                                if (solidTexture == null)
                                {
                                    _activeDrawingContext.FillTriangle(
                                        Triangle2D.Transform(tri, matrix),
                                        color);
                                }
                                else
                                {
                                    var destTri = Triangle2D.Transform(tri, matrix);
                                    var coordTri = new Triangle2D(new Vector2(tri.V0.X / 100.0f * solidTexture.Width,
                                                                              tri.V0.Y / 100.0f * solidTexture.Height),
                                                                  new Vector2(tri.V1.X / 100.0f * solidTexture.Width,
                                                                              tri.V1.Y / 100.0f * solidTexture.Height),
                                                                  new Vector2(tri.V2.X / 100.0f * solidTexture.Width,
                                                                              tri.V2.Y / 100.0f * solidTexture.Height));

                                    _activeDrawingContext.FillTriangle(
                                        solidTexture,
                                        coordTri,
                                        destTri,
                                        new ColorRgbaF(1.0f, 1.0f, 1.0f, color.A));
                                }
                            }
                            break;
                        }

                    case GeometryTexturedTriangles tt:
                        {
                            var coordinatesTransform = new Matrix3x2
                            {
                                M11 = tt.RotScale.M11,
                                M12 = tt.RotScale.M12,
                                M21 = tt.RotScale.M21,
                                M22 = tt.RotScale.M22,
                                M31 = tt.Translation.X,
                                M32 = tt.Translation.Y
                            };

                            foreach (var tri in tt.Triangles)
                            {
                                //if (assignment is RectangleAssignment)
                                //    throw new NotImplementedException();

                                var tex = _aptContext.GetTexture(tt.Image, shape);

                                _activeDrawingContext.FillTriangle(
                                    tex,
                                    Triangle2D.Transform(tri, coordinatesTransform),
                                    Triangle2D.Transform(tri, matrix),
                                    transform.ColorTransform);
                            }
                            break;
                        }
                }
            }
        }

        public void SetWindowSize(Size windowSize)
        {
            WindowSize = windowSize;
        }
    }
}
