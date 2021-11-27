using System.Numerics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class IncomingRoadData
    {
        public IncomingRoadData(
            RoadTopologyEdge topologyEdge,
            RoadSegmentEndPoint targetEndPoint,
            in Vector3 outDirection,
            float angleToAxis)
        {
            TopologyEdge = topologyEdge;
            TargetEndPoint = targetEndPoint;
            OutDirection = outDirection;
            AngleToAxis = angleToAxis;
        }

        public RoadTopologyEdge TopologyEdge { get; }
        public RoadSegmentEndPoint TargetEndPoint { get;}
        public Vector3 OutDirection { get; }
        public float AngleToAxis { get; }
        public IncomingRoadData Previous { get; internal set; }
        public float AngleToPreviousEdge { get; internal set; } = float.NaN;

        public Vector3 TargetNodePosition => TargetEndPoint.Position;
    }
}
