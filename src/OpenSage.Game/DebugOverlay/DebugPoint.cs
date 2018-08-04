using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.DebugOverlay
{
    public class DebugPoint : DebuggablePoint
    {
        public DebugPoint(Vector3 position) : base(position)
        {
        }

        public DebugPoint(Vector3 position, ColorRgbaF displayColor) : base(position, displayColor)
        {
        }
    }
}
