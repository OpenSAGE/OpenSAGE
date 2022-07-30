using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;

namespace OpenSage.Gui.DebugUI
{
    public static class DebugDrawingUtils
    {
        public static void DrawLine(DrawingContext2D context, Camera camera, in Vector3 start, in Vector3 end, in ColorRgbaF color)
        {
            var startScreen3D = camera.WorldToScreenPoint(start);
            var endScreen3D = camera.WorldToScreenPoint(end);

            if (!camera.IsWithinViewportDepth(startScreen3D) || !camera.IsWithinViewportDepth(endScreen3D))
            {
                return;
            }

            context.DrawLine(new Line2D(startScreen3D.Vector2XY(), endScreen3D.Vector2XY()), 1, color);
        }
    }
}
