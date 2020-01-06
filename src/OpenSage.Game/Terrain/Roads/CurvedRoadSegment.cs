using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class CurvedRoadSegment : IRoadSegment
    {
        public RoadSegmentEndPoint Start { get; }
        public RoadSegmentEndPoint End { get; }
        public Vector3 StartPosition => Start.Position;
        public Vector3 EndPosition => End.Position;
        public bool MirrorTexture => false;
        public RoadTextureType Type => RoadTextureType.BroadCurve;

        public CurvedRoadSegment(in Vector3 start, in Vector3 end, float width)
        {
            Start = new RoadSegmentEndPoint(start);
            End = new RoadSegmentEndPoint(end);
        }

        public IEnumerable<RoadSegmentEndPoint> EndPoints
        {
            get
            {
                yield return Start;
                yield return End;
            }
        }

        public RoadSegmentMesher CreateMesher(RoadTemplate template)
        {
            var halfWidth = template.RoadWidth * template.RoadWidthInTexture / 2f;
            return new StraightRoadSegmentMesher(this, halfWidth, template);
        }

        public static void CreateCurve(
            IReadOnlyList<IncomingRoadData> incomingRoadData,
            in Vector3 position,
            RoadTemplate template,
            IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            Debug.Assert(incomingRoadData.Count == 2);
            var startEdge = incomingRoadData.OrderBy(r => r.AngleToPreviousEdge).First();
            var endEdge = startEdge.Previous;

            var type = ChooseCurveType(startEdge.TopologyEdge, endEdge.TopologyEdge, position);

            switch (type)
            {
                case RoadTextureType.BroadCurve:
                case RoadTextureType.TightCurve:
                    CreateCurve(startEdge, endEdge, position, template, type, edgeSegments);
                    break;
            }
        }

        private static RoadTextureType ChooseCurveType(RoadTopologyEdge startEdge, RoadTopologyEdge endEdge, in Vector3 position)
        {
            // TODO: consider edge directions
            var startType = startEdge.Start.Position == position ? startEdge.StartType : startEdge.EndType;
            var endType = endEdge.Start.Position == position ? endEdge.StartType : endEdge.EndType;

            var type = startType & endType;
            if (type.HasFlag(RoadType.TightCurve))
            {
                return RoadTextureType.TightCurve;
            }
            else if (!type.HasFlag(RoadType.Angled))
            {
                return RoadTextureType.BroadCurve;
            }
            else
            {
                return RoadTextureType.Straight;
            }
        }

        private static void CreateCurve(
            IncomingRoadData startEdge,
            IncomingRoadData endEdge,
            Vector3 position,
            RoadTemplate template,
            RoadTextureType type,
            IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var curveAngle = MathF.PI - startEdge.AngleToPreviousEdge;

            // TODO figure out exact threshold
            if (curveAngle < 0.47f)
            {
                // render as angled connection
                return;
            }
            
            var halfRoadWidth = template.RoadWidth * template.RoadWidthInTexture / 2f;

            var radius = 3.5f * halfRoadWidth;
            var toCenterLength = radius / MathF.Cos(curveAngle / 2);
            var toCenterDirection = Vector3.Normalize(startEdge.OutDirection + endEdge.OutDirection);
            var toCenter = toCenterDirection * toCenterLength;
            var center = position + toCenter;

            var startSegmentEndDistance = Vector3.Dot(toCenter, startEdge.OutDirection);
            var endSegmentStartDistance = Vector3.Dot(toCenter, endEdge.OutDirection);

            var startEdgeVector = startEdge.TopologyEdge.End.Position - startEdge.TopologyEdge.Start.Position;
            var endEdgeVector = endEdge.TopologyEdge.End.Position - endEdge.TopologyEdge.Start.Position;

            if (startSegmentEndDistance * startSegmentEndDistance > startEdgeVector.LengthSquared() ||
                endSegmentStartDistance * endSegmentStartDistance > endEdgeVector.LengthSquared())
            {
                // render as angled connection
                return;
            }

            var startSegment = edgeSegments[startEdge.TopologyEdge];
            var endSegment = edgeSegments[endEdge.TopologyEdge];

            var startSegmentEndPoint = startSegment.StartPosition == position ? startSegment.Start : startSegment.End;
            startSegmentEndPoint.Position = position + startEdge.OutDirection * startSegmentEndDistance;
            startSegmentEndPoint.ConnectTo(endSegment, startEdge.OutDirection);

            var endSegmentStartPoint = endSegment.StartPosition == position ? endSegment.Start : endSegment.End;
            endSegmentStartPoint.Position = position + endEdge.OutDirection * endSegmentStartDistance;
            endSegmentStartPoint.ConnectTo(startSegment, endEdge.OutDirection);

            const float segmentAngle = MathF.PI / 6f;
            var numberOfSegments = 1 + (int) MathF.Floor(curveAngle / segmentAngle);
            var lastSegmentAngle = curveAngle % segmentAngle;

            var endpoints = GetSegmentEndpoints();

            ReadOnlySpan<Vector3> GetSegmentEndpoints()
            {
                var center2 = center.Vector2XY();
                var centerLeft = new Vector3(Vector2Utility.RotateAroundPoint(center2, position.Vector2XY(), curveAngle / 2f), 0f);
                var upDirection = Vector3.Normalize(center - centerLeft);

                var cosine = MathF.Cos(segmentAngle / 2);
                var additionalRadius = radius * (1f - cosine) / cosine;

                var topLeft = centerLeft + upDirection * halfRoadWidth;
                var bottomLeft = centerLeft - upDirection * (halfRoadWidth + additionalRadius);
                var topRight = new Vector3(Vector2Utility.RotateAroundPoint(center2, topLeft.Vector2XY(), curveAngle), 0f);
                var bottomRight = new Vector3(Vector2Utility.RotateAroundPoint(center2, bottomLeft.Vector2XY(), curveAngle), 0f);

                var endpoints = new Vector3[]
                {
                    topLeft, topRight,
                    bottomLeft, bottomRight
                };

                return endpoints.AsSpan();
            }
        }
    }
}
