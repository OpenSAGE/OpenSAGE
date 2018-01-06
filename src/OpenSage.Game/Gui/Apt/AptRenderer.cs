using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt;
using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Gui.Apt
{
    public sealed class AptRenderer
    {
        public static void RenderGeometry(DrawingContext drawingContext,AptContext context,
            Geometry shape, Matrix3x2 transform)
        {
            foreach (var e in shape.Entries)
            {
                switch (e)
                {
                    case GeometryLines l:
                        foreach (var line in l.Lines)
                        {
                            RawLineF rl;
                            rl.X1 = line.V0.X;
                            rl.Y1 = line.V0.Y;
                            rl.X2 = line.V1.X;
                            rl.Y2 = line.V1.Y;
                            rl.Thickness = l.Thickness;
                            drawingContext.DrawLine(rl, l.Color.ToColorRgbaF());
                        }
                        break;
                    case GeometrySolidTriangles st:
                        foreach (var tri in st.Triangles)
                        {
                            RawTriangleF rt;
                            rt.X1 = tri.V0.X;
                            rt.Y1 = tri.V0.Y;
                            rt.X2 = tri.V1.X;
                            rt.Y2 = tri.V1.Y;
                            rt.X3 = tri.V2.X;
                            rt.Y3 = tri.V2.Y;
                            drawingContext.FillTriangle(rt, st.Color.ToColorRgbaF());
                        }
                        break;
                    case GeometryTexturedTriangles tt:

                        foreach (var tri in tt.Triangles)
                        {
                            RawTriangleF rt;
                            rt.X1 = tri.V0.X;
                            rt.Y1 = tri.V0.Y;
                            rt.X2 = tri.V1.X;
                            rt.Y2 = tri.V1.Y;
                            rt.X3 = tri.V2.X;
                            rt.Y3 = tri.V2.Y;
                            RawMatrix3x2 brushTransform;
                            brushTransform.M11 = tt.RotScale.M11;
                            brushTransform.M12 = tt.RotScale.M12;
                            brushTransform.M21 = tt.RotScale.M21;
                            brushTransform.M22 = tt.RotScale.M22;
                            brushTransform.M31 = -tt.Translation.X;
                            brushTransform.M32 = -tt.Translation.Y;

                            //if (assignment is RectangleAssignment)
                            //    throw new NotImplementedException();

                            var tex = context.GetTexture(tt.Image, shape);

                            drawingContext.FillTriangle(rt, tex, brushTransform);
                        }
                        break;
                }
            }
        }
    }
}
