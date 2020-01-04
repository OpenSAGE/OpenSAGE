using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class CrossingRoadSegment : IRoadSegment
    {
        private const float OverlapLength = 0.015f;

        private CrossingRoadSegment(Vector3 position, IEnumerable<RoadSegmentEndPoint> endPoints, Vector3 start, Vector3 end, RoadTextureType type)
        {
            Position = position;
            EndPoints = endPoints;
            StartPosition = start;
            EndPosition = end;
            Type = type;
        }

        public Vector3 StartPosition { get; }
        public Vector3 EndPosition { get; }
        private Vector3 Position { get; }
        public IEnumerable<RoadSegmentEndPoint> EndPoints { get; }
        public RoadTextureType Type { get; }
        public bool MirrorTexture { get; set; } = false;

        public RoadSegmentMesher CreateMesher(RoadTemplate template)
        {
            var targetSize = GetSize(Type, template);
            var halfHeight = targetSize.Height / 2;

            return new CrossingRoadMesher(this, halfHeight, template);
        }

        private static SizeF GetSize(RoadTextureType type, RoadTemplate template)
        {
            var stubLength = 0.5f * (1f - template.RoadWidthInTexture);

            float width, height;

            switch (type)
            {
                case RoadTextureType.TCrossing:
                    width = template.RoadWidthInTexture + stubLength + OverlapLength;
                    height = template.RoadWidthInTexture + 2 * stubLength + 2 * OverlapLength;
                    break;
                case RoadTextureType.XCrossing:
                    width = template.RoadWidthInTexture + 2 * stubLength + 2 * OverlapLength;
                    height = width;
                    break;
                case RoadTextureType.AsymmetricYCrossing:
                    width = 1.2f + template.RoadWidthInTexture / 2f;
                    height = 1.33f + OverlapLength;
                    break;
                case RoadTextureType.SymmetricYCrossing:
                    width = 1.59f;
                    height = 1.065f + OverlapLength;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown RoadTextureType: " + type);
            }

            return new SizeF(width * template.RoadWidth, height * template.RoadWidth);
        }

        public static void CreateCrossing(
            IEnumerable<IncomingRoadData> roads,
            Vector3 crossingPosition, RoadTemplate template,
            IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var type = ChooseCrossingType(roads);
            switch(type)
            {
                case RoadTextureType.TCrossing:
                    CreateTCrossing(roads, crossingPosition, template, edgeSegments);
                    break;
                case RoadTextureType.AsymmetricYCrossing:
                    CreateYAsymmCrossing(roads, crossingPosition, template, edgeSegments);
                    break;
                case RoadTextureType.XCrossing:
                    CreateXCrossing(roads, crossingPosition, template, edgeSegments);
                    break;
                case RoadTextureType.SymmetricYCrossing:
                    // TODO
                    break;
                default:
                    throw new ArgumentException("Failed to choose crossing type", nameof(roads));
            }
        }

        private static RoadTextureType ChooseCrossingType(IEnumerable<IncomingRoadData> incomingRoads)
        {
            var angles = incomingRoads
                .Select(r => r.AngleToPreviousEdge)
                .OrderBy(a => a)
                .ToList();

            switch (angles.Count)
            {
                case 4:
                    return RoadTextureType.XCrossing;

                case 3:
                    if (angles[2] < Math.PI * 0.9)
                    {
                        return RoadTextureType.SymmetricYCrossing;
                    }
                    else if (angles[1] - angles[0] < Math.PI * 0.25)
                    {
                        return RoadTextureType.TCrossing;
                    }
                    else
                    {
                        return RoadTextureType.AsymmetricYCrossing;
                    }

                default:
                    throw new ArgumentException($"Cannot create crossing for {angles.Count} incoming roads", nameof(incomingRoads));
            }            
        }

        private static CrossingRoadSegment CreateTCrossing(
            IEnumerable<IncomingRoadData> roads,
            in Vector3 crossingPosition,
            RoadTemplate template,
            IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var maxAngle = roads.OrderBy(road => road.AngleToPreviousEdge).Last();
            var upDirection = Vector3.Normalize(maxAngle.Previous.TargetNodePosition - maxAngle.TargetNodePosition);
            var rightDirection = Vector3.Cross(upDirection, Vector3.UnitZ);

            var roadWidth = template.RoadWidth * template.RoadWidthInTexture;
            var halfRoadWidth = roadWidth / 2f;

            var targetSize = GetSize(RoadTextureType.TCrossing, template);

            var top = new RoadSegmentEndPoint(crossingPosition + targetSize.Height / 2 * upDirection);
            var bottom = new RoadSegmentEndPoint(crossingPosition - targetSize.Height / 2 * upDirection);
            var right = new RoadSegmentEndPoint(crossingPosition + (targetSize.Width - halfRoadWidth) * rightDirection);

            var start = crossingPosition - halfRoadWidth * rightDirection;
            var end = right.Position;

            var crossingSegment = new CrossingRoadSegment(
                crossingPosition,
                new[] { top, bottom, right },
                start,
                end,
                RoadTextureType.TCrossing);

            var overlap = OverlapLength * template.RoadWidth;
            Connect(crossingSegment, maxAngle.Previous.TopologyEdge, top, upDirection, overlap, edgeSegments);
            Connect(crossingSegment, maxAngle.Previous.Previous.TopologyEdge, right, rightDirection, overlap, edgeSegments);
            Connect(crossingSegment, maxAngle.TopologyEdge, bottom, -upDirection, overlap, edgeSegments);

            return crossingSegment;
        }

        private static CrossingRoadSegment CreateXCrossing(
            IEnumerable<IncomingRoadData> roads,
            Vector3 crossingPosition,
            RoadTemplate template,
            IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var roadBottom = roads.First();
            var roadLeft = roadBottom.Previous;
            var roadTop = roadLeft.Previous;
            var roadRight = roadTop.Previous;
            Debug.Assert(roadBottom == roadRight.Previous);

            //compare normal of the average direction of the horizontal road
            //with direction of the vertical road
            //take the average as the up direction for the crossing
            var upDir1 = Vector3.Cross(Vector3.UnitZ, Vector3.Normalize(roadRight.TargetNodePosition - roadLeft.TargetNodePosition));
            var upDir2 = Vector3.Normalize(roadTop.TargetNodePosition - roadBottom.TargetNodePosition);
            var upDirection = 0.5f * (upDir1 + upDir2);
            var rightDirection = Vector3.Cross(upDirection, Vector3.UnitZ);

            var targetSize = GetSize(RoadTextureType.XCrossing, template);

            var top = new RoadSegmentEndPoint(crossingPosition + targetSize.Height / 2 * upDirection);
            var bottom = new RoadSegmentEndPoint(crossingPosition - targetSize.Height / 2 * upDirection);
            var right = new RoadSegmentEndPoint(crossingPosition + targetSize.Height / 2 * rightDirection);
            var left = new RoadSegmentEndPoint(crossingPosition - targetSize.Height / 2 * rightDirection);

            var start = left.Position;
            var end = right.Position;

            var crossingSegment = new CrossingRoadSegment(
                crossingPosition,
                new[] { top, bottom, right, left },
                start,
                end,
                RoadTextureType.XCrossing);

            var overlap = OverlapLength * template.RoadWidth;
            Connect(crossingSegment, roadTop.TopologyEdge, top, upDirection, overlap, edgeSegments);
            Connect(crossingSegment, roadRight.TopologyEdge, right, rightDirection, overlap, edgeSegments);
            Connect(crossingSegment, roadBottom.TopologyEdge, bottom, -upDirection, overlap, edgeSegments);
            Connect(crossingSegment, roadLeft.TopologyEdge, left, -rightDirection, overlap, edgeSegments);

            return crossingSegment;
        }

        private static CrossingRoadSegment CreateYAsymmCrossing(
            IEnumerable<IncomingRoadData> roads,
            Vector3 crossingPosition,
            RoadTemplate template,
            IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var maxAngle = roads.OrderBy(road => road.AngleToPreviousEdge).Last();
            var mirror = maxAngle.Previous.AngleToPreviousEdge < maxAngle.Previous.Previous.AngleToPreviousEdge;
            
            var upDirection = Vector3.Normalize(maxAngle.Previous.TargetNodePosition - maxAngle.TargetNodePosition);
            var rightDirection = Vector3.Cross(upDirection, Vector3.UnitZ);
            var mirrorFactor = mirror ? -1 : 1;
            upDirection = mirrorFactor * upDirection;
            var sideDirection = Vector3.Normalize(rightDirection - upDirection);

            var targetSize = GetSize(RoadTextureType.AsymmetricYCrossing, template);

            var lengthToTop = 0.2f * targetSize.Height;
            var lengthToBottom = 0.8f * targetSize.Height;
            var halfRoadWidth = template.RoadWidth * template.RoadWidthInTexture / 2f;
            var lengthToLeft = halfRoadWidth;
            var lengthToRight = targetSize.Width - lengthToLeft;
            var lengthToSide = 1.047f * template.RoadWidth;

            var topPosition = crossingPosition + lengthToTop * upDirection;
            var bottomPosition = crossingPosition - lengthToBottom * upDirection;
            var sidePosition = crossingPosition + lengthToSide * sideDirection;

            var midHeightOnMainRoad = 0.5f * (topPosition + bottomPosition);
            var start = midHeightOnMainRoad - lengthToLeft * rightDirection;
            var end = midHeightOnMainRoad + lengthToRight * rightDirection;

            var top = new RoadSegmentEndPoint(topPosition);
            var bottom = new RoadSegmentEndPoint(bottomPosition);
            var side = new RoadSegmentEndPoint(sidePosition);

            var crossingSegment = new CrossingRoadSegment(
                crossingPosition,
                new[] { top, bottom, side },
                start,
                end,
                RoadTextureType.AsymmetricYCrossing);

            var topEdge = mirror ? maxAngle.TopologyEdge : maxAngle.Previous.TopologyEdge;
            var bottomEdge = mirror ? maxAngle.Previous.TopologyEdge : maxAngle.TopologyEdge;

            Connect(crossingSegment, topEdge, top, upDirection, 0.04f * template.RoadWidth, edgeSegments);
            Connect(crossingSegment, maxAngle.Previous.Previous.TopologyEdge, side, sideDirection, 0, edgeSegments);
            Connect(crossingSegment, bottomEdge, bottom, -upDirection, 0.055f * template.RoadWidth, edgeSegments);

            crossingSegment.MirrorTexture = mirror;

            return crossingSegment;
        }

        private static void Connect(
            CrossingRoadSegment newSegment,
            RoadTopologyEdge edge,
            RoadSegmentEndPoint endPoint,
            in Vector3 direction,
            float overlap,
            IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            endPoint.Position -= overlap * direction;
            var edgeSegment = edgeSegments[edge];
            if (edge.Start.Position == newSegment.Position)
            {
                edgeSegment.Start.Position = endPoint.Position;
                edgeSegment.Start.ConnectTo(newSegment, direction);
                endPoint.ConnectTo(edgeSegment, edge.Start.Position - edge.End.Position);
            }
            else
            {
                edgeSegment.End.Position = endPoint.Position;
                edgeSegment.End.ConnectTo(newSegment, direction);
                endPoint.ConnectTo(edgeSegment, edge.End.Position - edge.Start.Position);
            }
        }
    }
}
