using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Apt
{
    public sealed class AptRenderer
    {
        public AptWindow Window { get; }
        private readonly ContentManager _contentManager;

        private void CalculateTransform(ref ItemTransform transform, AptContext context)
        {
            var movie = (Movie) context.Root.Character;
            var movieSize = new Vector2(movie.ScreenWidth, movie.ScreenHeight);

            var scaling = Window.GetScaling();
            transform.GeometryRotation.M11 *= scaling.X;
            transform.GeometryRotation.M22 *= scaling.Y;
            transform.GeometryRotation.Translation = transform.GeometryTranslation * scaling;
        }

        public AptRenderer(AptWindow window, ContentManager contentManager)
        {
            _contentManager = contentManager;
            Window = window;
        }

        public void RenderText(DrawingContext2D drawingContext, AptContext context,
            Text text, ItemTransform transform)
        {
            var font = _contentManager.FontManager.GetOrCreateFont("Arial", text.FontHeight, FontWeight.Normal);
            CalculateTransform(ref transform, context);

            drawingContext.DrawText(
                text.Content,
                font,
                TextAlignment.Center,
                text.Color.ToColorRgbaF() * transform.ColorTransform,
                RectangleF.Transform(text.Bounds, transform.GeometryRotation));
        }

        public void RenderGeometry(DrawingContext2D drawingContext, AptContext context,
            Geometry shape, ItemTransform transform, Texture solidTexture)
        {
            CalculateTransform(ref transform, context);
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
                                drawingContext.DrawLine(
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
                                    drawingContext.FillTriangle(
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

                                    drawingContext.FillTriangle(
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

                                var tex = context.GetTexture(tt.Image, shape);

                                drawingContext.FillTriangle(
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
    }
}
