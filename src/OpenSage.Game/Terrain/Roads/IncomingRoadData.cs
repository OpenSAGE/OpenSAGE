using System.Numerics;
using OpenSage.Data.Map;

namespace OpenSage.Terrain.Roads
{
    internal sealed class IncomingRoadData
    {
        public IncomingRoadData(
            RoadTopologyEdge topologyEdge,
            in Vector3 targetNodePosition,
            in Vector3 outDirection,
            float angleToAxis)
        {
            TopologyEdge = topologyEdge;
            TargetNodePosition = targetNodePosition;
            OutDirection = outDirection;
            AngleToAxis = angleToAxis;
        }

        public RoadTopologyEdge TopologyEdge { get; }
        public Vector3 TargetNodePosition { get;}
        public Vector3 OutDirection { get; }
        public float AngleToAxis { get; }
        public IncomingRoadData Previous { get; internal set; }
        public float AngleToPreviousEdge { get; internal set; } = float.NaN;
    }
}
