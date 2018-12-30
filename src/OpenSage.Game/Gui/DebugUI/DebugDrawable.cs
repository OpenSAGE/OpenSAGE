using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;

namespace OpenSage.Gui.DebugUI
{
    interface IDebugDrawable
    {
        /// <summary>
        /// The time this drawable has left before deletion in seconds.
        /// When the timer reaches 0, it will be deleted at the end of the frame.
        /// If the timer value is null, the drawable will stay permanently.
        /// </summary>
        float? Timer { get; set; }
        void Render(DrawingContext2D context, Camera camera);
    }

    public class DebugPoint : IDebugDrawable
    {
        public readonly Vector3 Position;
        public readonly ColorRgbaF Color;
        public float? Timer { get; set; }

        public DebugPoint(in Vector3 position, in ColorRgbaF color, float? duration = null)
        {
            Position = position;
            Color = color;
            Timer = duration;
        }

        public void Render(DrawingContext2D context, Camera camera)
        {
            var rect = camera.WorldToScreenRectangle(Position, new SizeF(4.0f));
            if (rect.HasValue)
            {
                context.DrawRectangle(rect.Value, Color, 1);
            }
        }
    }

    public class DebugCoordAxes : IDebugDrawable
    {
        public readonly Vector3 Position;
        public float? Timer { get; set; }

        public DebugCoordAxes(in Vector3 position, float? duration = null)
        {
            Position = position;
            Timer = duration;
        }

        public void Render(DrawingContext2D context, Camera camera)
        {
            DebugDrawingUtils.DrawLine(context, camera, Position, Position + Vector3.UnitX, new ColorRgbaF(1, 0, 0, 1));
            DebugDrawingUtils.DrawLine(context, camera, Position, Position + Vector3.UnitY, new ColorRgbaF(0, 1, 0, 1));
            DebugDrawingUtils.DrawLine(context, camera, Position, Position + Vector3.UnitZ, new ColorRgbaF(0, 0, 1, 1));
        }
    }

    public class DebugLine : IDebugDrawable
    {
        public readonly Vector3 Start;
        public readonly Vector3 End;
        public readonly ColorRgbaF Color;
        public float? Timer { get; set; }

        public DebugLine(in Vector3 start, in Vector3 end, in ColorRgbaF color, float? duration = null)
        {
            Start = start;
            End = end;
            Color = color;
            Timer = duration;
        }

        public void Render(DrawingContext2D context, Camera camera)
        {
            DebugDrawingUtils.DrawLine(context, camera, Start, End, Color);
        }
    }

    internal static class DebugDrawingUtils
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
