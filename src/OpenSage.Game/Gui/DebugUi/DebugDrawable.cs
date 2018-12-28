using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;

namespace OpenSage.Gui.DebugUi
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

        public DebugPoint(Vector3 position, ColorRgbaF color, float? duration = null)
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

    public class DebugLine : IDebugDrawable
    {
        public readonly Vector3 Start;
        public readonly Vector3 End;
        public readonly ColorRgbaF Color;
        public float? Timer { get; set; }

        public DebugLine(Vector3 start, Vector3 end, ColorRgbaF color, float? duration = null)
        {
            Start = start;
            End = end;
            Color = color;
            Timer = duration;
        }

        public void Render(DrawingContext2D context, Camera camera)
        {
            var startScreen3D = camera.WorldToScreenPoint(Start);
            var endScreen3D = camera.WorldToScreenPoint(End);

            if (!camera.IsWithinViewportDepth(startScreen3D) || !camera.IsWithinViewportDepth(endScreen3D))
            {
                return;
            }

            context.DrawLine(new Line2D(startScreen3D.Vector2XY(), endScreen3D.Vector2XY()), 1.0f, Color);
        }
    }
}
