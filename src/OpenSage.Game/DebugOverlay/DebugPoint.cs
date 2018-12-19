using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;

namespace OpenSage.DebugOverlay
{
    /// <summary>
    /// A point in world space with a color.
    /// </summary>
    public struct DebugPoint
    {
        public readonly Vector3 Position;
        public readonly ColorRgbaF Color;

        public DebugPoint(Vector3 position) : this(position, ColorRgbaF.Blue) { }

        public DebugPoint(Vector3 position, ColorRgbaF color)
        {
            Position = position;
            Color = color;
        }

        private static readonly BoundingBox Bounds = new BoundingBox(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));

        public Rectangle GetBoundingRectangle(Camera camera)
        {
            var worldBounds = BoundingBox.Transform(Bounds, Matrix4x4.CreateTranslation(Position));
            return worldBounds.GetBoundingRectangle(camera);
        }

        public bool Intersects(in BoundingFrustum frustum)
        {
            var worldBounds = BoundingBox.Transform(Bounds, Matrix4x4.CreateTranslation(Position));
            return frustum.Intersects(worldBounds);
        }
    }
}
