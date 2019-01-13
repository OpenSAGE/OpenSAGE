using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public sealed class AptRenderer
    {
        private Vector2 _outputSize;
        private readonly ContentManager _contentManager;

        private void CalculateTransform(ref ItemTransform transform, AptContext context)
        {
            var movie = (Movie) context.Root.Character;
            var movieSize = new Vector2(movie.ScreenWidth, movie.ScreenHeight);

            var scaling = _outputSize / movieSize;
            transform.GeometryRotation.M11 *= scaling.X;
            transform.GeometryRotation.M22 *= scaling.Y;
            transform.GeometryRotation.Translation = transform.GeometryTranslation * scaling;
        }

        public AptRenderer(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public void RenderText(DrawingContext2D drawingContext, AptContext context,
            Text text, ItemTransform transform)
        {
            var font = _contentManager.GetOrCreateFont("Arial", text.FontHeight, FontWeight.Normal);
            CalculateTransform(ref transform, context);

            drawingContext.DrawText(
                text.Content,
                font,
                TextAlignment.Center,
                text.Color.ToColorRgbaF() * transform.ColorTransform,
                RectangleF.Transform(text.Bounds, transform.GeometryRotation));
        }

        public void RenderGeometry(DrawingContext2D drawingContext, AptContext context,
            Geometry shape, ItemTransform transform)
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
                                drawingContext.FillTriangle(
                                    Triangle2D.Transform(tri, matrix),
                                    color);
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

        public void Resize(in Size outputSize)
        {
            _outputSize.X = outputSize.Width;
            _outputSize.Y = outputSize.Height;
        }
    }
}
