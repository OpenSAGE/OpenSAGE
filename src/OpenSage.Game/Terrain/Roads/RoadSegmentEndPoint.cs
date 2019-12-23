using System.Numerics;

namespace OpenSage.Terrain.Roads
{
    internal class RoadSegmentEndPoint
    {
        public RoadSegmentEndPoint(in Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }
    }
}
