using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Gui.DebugUi
{
    /// <summary>
    /// A point in world space with a color.
    /// </summary>
    public readonly struct DebugPoint
    {
        public readonly Vector3 Position;
        public readonly ColorRgbaF Color;

        public DebugPoint(Vector3 position) : this(position, ColorRgbaF.Blue) { }

        public DebugPoint(Vector3 position, ColorRgbaF color)
        {
            Position = position;
            Color = color;
        }
    }
}
