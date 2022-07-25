using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Orders
{
    /// <remarks/>
    public class OrderArgumentValue
    {
        public int Integer { get; set; }

        public float Float { get; set; }

        public bool Boolean { get; set; }

        public uint ObjectId { get; set; }

        public Vector3Wrapper Position { get; set; }

        public Point2D ScreenPosition { get; set; }

        public RectangleWrapper ScreenRectangle { get; set; }
    }
}
