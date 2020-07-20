using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class EndCapRoadSegment : IRoadSegment
    {
        internal class EndCapData
        {
            public EndCapData(float angle, in Vector3 incomingDirection, float? squaredNormalDistanceFromJoinedSegment)
            {
                Angle = angle;
                IncomingDirection = incomingDirection;
                SquaredNormalDistanceFromJoinedSegment = squaredNormalDistanceFromJoinedSegment;
            }

            public float Angle { get; }
            public Vector3 IncomingDirection { get; }         
            public float? SquaredNormalDistanceFromJoinedSegment { get; }
            public bool DoesJoinOtherSegment => SquaredNormalDistanceFromJoinedSegment.HasValue;
        }

        public EndCapRoadSegment(RoadSegmentEndPoint endPoint, in Vector3 startPosition, in Vector3 endPosition, EndCapData data)
        {
            EndPoint = endPoint;
            StartPosition = startPosition;
            EndPosition = endPosition;
            Data = data;
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

        public EndCapData Data { get; }

        public RoadSegmentMesher CreateMesher(RoadTemplate template)
        {
            return new EndCapRoadSegmentMesher(this, template, Data.Angle);
        }

        public static void CreateEndCap(
            IncomingRoadData incomingRoadData,
            Vector3 position,
            RoadTemplate template,
            IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var incomingSegment = edgeSegments[incomingRoadData.TopologyEdge];
            var incomingEndPoint = incomingSegment.StartPosition == position ? incomingSegment.Start : incomingSegment.End;

            var joinableRoadSegmentAngles = from segment in edgeSegments
                                            where segment.Key.Template != template
                                            let result = CanJoin(segment.Value, segment.Key.Template)
                                            where result != null
                                            orderby result.SquaredNormalDistanceFromJoinedSegment
                                            select result;

            var endCapData = joinableRoadSegmentAngles.FirstOrDefault() ?? new EndCapData(0f, incomingRoadData.OutDirection, null);

            var endCapLength = template.RoadWidth * 3f / 8f;
            var overlapDistance = template.RoadWidth * 0.13f;

            var startPosition = position + endCapData.IncomingDirection * overlapDistance;
            var endPosition = startPosition - endCapData.IncomingDirection * endCapLength;

            var endPoint = new RoadSegmentEndPoint(position);
            var endCap = new EndCapRoadSegment(endPoint, startPosition, endPosition, endCapData);
            endPoint.ConnectTo(incomingSegment, -incomingRoadData.OutDirection);
            incomingEndPoint.ConnectTo(endCap, endCapData.IncomingDirection);

            EndCapData CanJoin(StraightRoadSegment segment, RoadTemplate segmentTemplate)
            {
                var segmentVector = segment.EndPosition - segment.StartPosition;
                var segmentDirection = Vector3.Normalize(segmentVector);
                var toCornerDirection = Vector3.Cross(segmentDirection, Vector3.UnitZ);
                var toCornerLength = segmentTemplate.RoadWidth * segmentTemplate.RoadWidthInTexture / 2;
                var additionalLength = template.RoadWidth / 10f; // we don't have to touch roads to be able to join them
                var totalLength = toCornerLength + additionalLength;
                var toCorner = toCornerDirection * totalLength;
                
                var boundingBox = BoundingBox.CreateFromPoints(
                    segment.StartPosition + toCorner,
                    segment.StartPosition - toCorner,
                    segment.EndPosition + toCorner,
                    segment.EndPosition - toCorner);

                if (!boundingBox.Contains(position))
                {
                    return null;
                }

                var startToPosition = position - segment.StartPosition;
                var squaredNormalDistance = Vector3.Cross(startToPosition, segmentDirection).LengthSquared();
                if (squaredNormalDistance > totalLength * totalLength)
                {
                    return null;
                }

                var signedDistanceFromStart = Vector3.Dot(startToPosition, segmentDirection);
                if (signedDistanceFromStart < -additionalLength || signedDistanceFromStart > segmentVector.Length() + additionalLength)
                {
                    return null;
                }

                var angle = MathF.Asin(Vector3.Dot(incomingRoadData.OutDirection, segmentDirection));
                if (MathF.Abs(angle) > MathF.PI / 4)
                {
                    return null;
                }

                var nearestPointOnSegment = segment.StartPosition + segmentDirection * signedDistanceFromStart;
                return new EndCapData(angle, Vector3.Normalize(position - nearestPointOnSegment), squaredNormalDistance);
            }
        }
    }
}
