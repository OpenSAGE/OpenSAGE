using System.Numerics;

namespace OpenSage.Terrain.Roads
{
    internal class RoadSegmentEndPoint
    {
        public RoadSegmentEndPoint(in Vector3 position)
        {
            Position = position;            
        }

        public Vector3 Position { get; set; }
        public Vector3 IncomingDirection { get; private set; }
        public IRoadSegment To { get; private set; }

        internal void ConnectTo(IRoadSegment segment, in Vector3 incomingDirection)
        {
            To = segment;
            IncomingDirection = incomingDirection;
        }
    }
}
