using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class CurvedRoadSegment : ISimpleRoadSegment
    {
        public RoadSegmentEndPoint Start { get; }
        public RoadSegmentEndPoint End { get; }
        public Vector3 StartPosition => 0.5f * (TopLeft + BottomLeft);
        public Vector3 EndPosition => 0.5f * (TopRight + BottomRight);
        public Vector3 TopLeft { get; }
        public Vector3 TopRight { get; }
        public Vector3 BottomLeft { get; }
        public Vector3 BottomRight { get; }

        public bool MirrorTexture => false;
        public RoadTextureType Type { get; }

        public CurvedRoadSegment(
            Vector3 topLeft,
            Vector3 topRight,
            Vector3 bottomLeft,
            Vector3 bottomRight,
            RoadSegmentEndPoint start,
            RoadSegmentEndPoint end,
            RoadTextureType type)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            Start = start;
            End = end;
            Type = type;
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
            return new CurvedRoadSegmentMesher(this, template);
        }

        public static void CreateCurve(
            IReadOnlyList<IncomingRoadData> incomingRoadData,
            Vector3 position,
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

        private static RoadTextureType ChooseCurveType(RoadTopologyEdge startEdge, RoadTopologyEdge endEdge, Vector3 position)
        {
            // This algorithm seems weird and I'm pretty sure this is not how it's done in the original games.
            // While it does seem to produce the same results, there's probably a less arbitrary and more concise
            // way to implement this. It might be connected to the alignment of the orientation (see RoadTopology.cs).

            var startType = startEdge.Start.Position == position ? startEdge.StartType : startEdge.EndType;
            var endType = endEdge.Start.Position == position ? endEdge.StartType : endEdge.EndType;

            var type = startType & endType;
            if (type.HasFlag(RoadType.TightCurve))
            {
                return RoadTextureType.TightCurve;
            }
            else if (type.HasFlag(RoadType.Angled))
            {
                return RoadTextureType.Straight;
            }
            else
            {
                var firstType = startEdge.Index < endEdge.Index ? startType : endType;
                if (firstType.HasFlag(RoadType.Angled))
                {
                    return RoadTextureType.Straight;
                }
                else
                {
                    return RoadTextureType.BroadCurve;
                }
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
            var innerAngle = startEdge.AngleToPreviousEdge;

            if (curveAngle < RoadConstants.MinCurveAngle)
            {
                // render as angled connection
                return;
            }

            var halfRoadWidth = template.RoadWidth * template.RoadWidthInTexture / 2f;

            var radius = (type == RoadTextureType.TightCurve ? RoadConstants.TightCurveRadius : 3.3f) * halfRoadWidth;
            var toCenterLength = radius / MathF.Sin(innerAngle / 2);
            var toCenterDirection = Vector3.Normalize(startEdge.OutDirection + endEdge.OutDirection);
            var toCenter = toCenterDirection * toCenterLength;
            var center = position + toCenter;
            var center2 = center.Vector2XY();

            var startSegmentEndDistance = Vector3.Dot(toCenter, startEdge.OutDirection);
            var endSegmentStartDistance = Vector3.Dot(toCenter, endEdge.OutDirection);

            var startSegment = edgeSegments[startEdge.TopologyEdge];
            var endSegment = edgeSegments[endEdge.TopologyEdge];

            var startEdgeVector = startSegment.End.Position - startSegment.Start.Position;
            var endEdgeVector = endSegment.End.Position - endSegment.Start.Position;

            var overlapAngle = MathUtility.ToRadians(type == RoadTextureType.TightCurve ? 6f : 2f);
            var overlapDistance = 2 * overlapAngle * radius;

            if (Math.Pow(startSegmentEndDistance - overlapDistance, 2) > startEdgeVector.LengthSquared() ||
                Math.Pow(endSegmentStartDistance - overlapDistance, 2) > endEdgeVector.LengthSquared())
            {
                // render as angled connection
                return;
            }

            var startSegmentEndPoint = startSegment.StartPosition == position ? startSegment.Start : startSegment.End;
            startSegmentEndPoint.Position = position + startEdge.OutDirection * startSegmentEndDistance;

            var endSegmentStartPoint = endSegment.StartPosition == position ? endSegment.Start : endSegment.End;
            endSegmentStartPoint.Position = position + endEdge.OutDirection * endSegmentStartDistance;

            const float segmentAngle = MathF.PI / 6f;
            var cosine = MathF.Cos(segmentAngle / 2);
            var additionalRadius = (radius + halfRoadWidth) * (1f - cosine) / cosine;

            var remainingAngle = curveAngle;
            var overlapRotationAngle = 0f;
            var previousEndPoint = startSegmentEndPoint;
            var previousDirection = -startEdge.OutDirection;
            IRoadSegment previousSegment = startSegment;
            CurvedRoadSegment currentSegment = null;

            while (remainingAngle > 0f)
            {
                var centerLeft = previousEndPoint.Position;
                centerLeft = new Vector3(Vector2Utility.RotateAroundPoint(center2, centerLeft.Vector2XY(), overlapRotationAngle), 0f);

                currentSegment = CreateSegment(centerLeft);
                currentSegment.Start.ConnectTo(previousSegment, previousDirection);
                previousEndPoint.ConnectTo(currentSegment, -previousDirection);

                previousSegment = currentSegment;
                previousEndPoint = currentSegment.End;
                previousDirection = Vector3.Cross(currentSegment.TopRight - currentSegment.BottomRight, Vector3.UnitZ);

                remainingAngle -= segmentAngle + overlapRotationAngle;
                overlapRotationAngle = -MathF.Max(overlapAngle, segmentAngle - remainingAngle);
            }

            currentSegment.End.ConnectTo(endSegment, -endEdge.OutDirection);
            endSegmentStartPoint.ConnectTo(currentSegment, endEdge.OutDirection);

            startSegmentEndPoint.Position -= startEdge.OutDirection * 2f * RoadConstants.OverlapLength * template.RoadWidth;
            endSegmentStartPoint.Position -= endEdge.OutDirection * 2f * RoadConstants.OverlapLength * template.RoadWidth;

            CurvedRoadSegment CreateSegment(Vector3 centerLeft)
            {
                var upDirection = Vector3.Normalize(center - centerLeft);

                var topLeft = centerLeft + upDirection * halfRoadWidth;
                var bottomLeft = centerLeft - upDirection * (halfRoadWidth + additionalRadius);
                var topRight = new Vector3(Vector2Utility.RotateAroundPoint(center2, topLeft.Vector2XY(), segmentAngle), 0f);
                var bottomRight = new Vector3(Vector2Utility.RotateAroundPoint(center2, bottomLeft.Vector2XY(), segmentAngle), 0f);

                var topRightToBottomRight = bottomRight - topRight;

                var startPoint = new RoadSegmentEndPoint(centerLeft);
                var endPoint = new RoadSegmentEndPoint(topRight + Vector3.Normalize(topRightToBottomRight) * halfRoadWidth);

                return new CurvedRoadSegment(topLeft, topRight, bottomLeft, bottomRight, startPoint, endPoint, type);
            }
        }
    }
}
