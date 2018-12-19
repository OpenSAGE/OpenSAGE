using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;

namespace OpenSage.DebugOverlay
{
    public class DebugPoint
    {
        private readonly Transform _transform;
        public Vector3 Position { get; }
        public ColorRgbaF DisplayColor { get; }
        private readonly BoundingBox _bounds;

        public DebugPoint(Vector3 position) : this(position, ColorRgbaF.Blue) { }

        public DebugPoint(Vector3 position, ColorRgbaF displayColor)
        {
            Position = position;
            _transform = new Transform(position, new Quaternion(0, 0, 0, 0));
            DisplayColor = displayColor;
            var min = new Vector3(0, 0, 0);
            var max = new Vector3(1, 1, 0);
            _bounds = new BoundingBox(min, max);
        }

        public virtual Rectangle GetBoundingRectangle(Camera camera)
        {
            var worldBounds = BoundingBox.Transform(_bounds, _transform.Matrix);
            return worldBounds.GetBoundingRectangle(camera);
        }

        public virtual bool Intersects(in BoundingFrustum frustum)
        {
            var worldBounds = BoundingBox.Transform(_bounds, _transform.Matrix);
            return frustum.Intersects(worldBounds);
        }
    }
}
