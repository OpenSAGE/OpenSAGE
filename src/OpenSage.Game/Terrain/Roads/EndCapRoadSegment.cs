using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class EndCapRoadSegment : IRoadSegment
    {
        public EndCapRoadSegment(RoadSegmentEndPoint endPoint, in Vector3 startPosition, in Vector3 endPosition)
        {
            EndPoint = endPoint;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }

        public Vector3 StartPosition { get; }

        public Vector3 EndPosition { get; }

        public RoadSegmentEndPoint EndPoint { get; }
        public RoadTemplate Template { get; }

        public RoadTextureType Type => RoadTextureType.EndCap;

        public bool MirrorTexture => false;        

        public IEnumerable<RoadSegmentEndPoint> EndPoints
        {
            get
            {
                yield return EndPoint;
            }
        }

        public RoadSegmentMesher CreateMesher(RoadTemplate template)
        {
            return new EndCapRoadSegmentMesher(this, template);
        }

        public static void CreateEndCap(
            IncomingRoadData incomingRoadData,
            in Vector3 position,
            RoadTemplate template,
            IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var incomingSegment = edgeSegments[incomingRoadData.TopologyEdge];
            var incomingEndPoint = incomingSegment.StartPosition == position ? incomingSegment.Start : incomingSegment.End;

            var endCapLength = template.RoadWidth * 3f / 8f;
            var overlapDistance = template.RoadWidth * 0.13f;
            var startPosition = position + incomingRoadData.OutDirection * overlapDistance;
            var endPosition = startPosition - incomingRoadData.OutDirection * endCapLength;

            var endPoint = new RoadSegmentEndPoint(position);
            var endCap = new EndCapRoadSegment(endPoint, startPosition, endPosition);
            endPoint.ConnectTo(incomingSegment, -incomingRoadData.OutDirection);
            incomingEndPoint.ConnectTo(endCap, incomingRoadData.OutDirection);            
        }
    }
}
