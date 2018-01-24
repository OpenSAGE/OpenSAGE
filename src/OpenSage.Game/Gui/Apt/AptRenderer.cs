using System.Numerics;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public sealed class AptRenderer
    {
        public static void RenderText(DrawingContext2D drawingContext, AptContext context,
            Text text, ItemTransform transform)
        {
            var content = context.ContentManager;
            var font = content.GetOrCreateFont("Arial", text.FontHeight, FontWeight.Normal);

            drawingContext.DrawText(
                text.Content,
                font,
                TextAlignment.Center,
                text.Color.ToColorRgbaF() * transform.ColorTransform,
                RectangleF.Transform(text.Bounds, transform.GeometryTransform));
        }

        public static void RenderGeometry(DrawingContext2D drawingContext, AptContext context,
            Geometry shape, ItemTransform transform)
        {
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
                                    Line2D.Transform(line, transform.GeometryTransform),
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
                                    Triangle2D.Transform(tri, transform.GeometryTransform),
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
                                    Triangle2D.Transform(tri, transform.GeometryTransform),
                                    transform.ColorTransform);
                            }
                            break;
                        }
                }
            }
        }
    }
}
