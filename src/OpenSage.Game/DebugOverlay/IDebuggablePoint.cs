using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;

namespace OpenSage.DebugOverlay
{
    public interface IDebuggablePoint
    {
        ColorRgbaF DisplayColor { get; }

        Rectangle GetBoundingRectangle(Camera camera);

        bool Intersects(in BoundingFrustum frustum);
    }
}
