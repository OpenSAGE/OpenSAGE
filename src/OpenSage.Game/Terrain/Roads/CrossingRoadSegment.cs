using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class IncomingRoadData
    {
        public RoadTopologyEdge TopologyEdge { get; set; }
        public Vector3 TargetNodePosition { get; set; }
        public double AngleToPreviousEdge { get; set; } = float.NaN;
        public IncomingRoadData Previous { get; set; }
        public Vector3 Direction { get; set; }
        public double AngleToAxis { get; set; } = float.NaN;
    }

    internal sealed class CrossingRoadSegment : IRoadSegment
    {
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



        public static CrossingRoadSegment CreateTCrossing(IEnumerable<IncomingRoadData> roads, Vector3 crossingPosition, RoadTemplate template, IDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var maxAngle = roads.OrderBy(road => road.AngleToPreviousEdge).LastOrDefault();
            var upDirection = Vector3.Normalize(maxAngle.Previous.TargetNodePosition - maxAngle.TargetNodePosition);
            var rightDirection = Vector3.Cross(upDirection, Vector3.UnitZ);

            var roadWidth = template.RoadWidth * template.RoadWidthInTexture;
            var halfRoadWidth = roadWidth / 2f;

            var targetBoundingBox = GetBoundingBoxSize(RoadTextureType.TCrossing, template);

            var top = new RoadSegmentEndPoint(crossingPosition + targetBoundingBox.Height / 2 * upDirection);
            var bottom = new RoadSegmentEndPoint(crossingPosition - targetBoundingBox.Height / 2 * upDirection);
            var right = new RoadSegmentEndPoint(crossingPosition + (targetBoundingBox.Width - halfRoadWidth) * rightDirection);

            var start = crossingPosition - halfRoadWidth * rightDirection;
            var end = right.Position;

            var crossingSegment = new CrossingRoadSegment(crossingPosition, new[] { top, bottom, right }, start, end, RoadTextureType.TCrossing);

            var overlap = 0.015f * template.RoadWidth;
            Connect(crossingSegment, maxAngle.Previous.TopologyEdge, top, upDirection, overlap, edgeSegments);
            Connect(crossingSegment, maxAngle.Previous.Previous.TopologyEdge, right, rightDirection, overlap, edgeSegments);
            Connect(crossingSegment, maxAngle.TopologyEdge, bottom, -upDirection, overlap, edgeSegments);

            return crossingSegment;
        }

        public static CrossingRoadSegment CreateYAsymmCrossing(IEnumerable<IncomingRoadData> roads, Vector3 crossingPosition, RoadTemplate template, IDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var maxAngle = roads.OrderBy(road => road.AngleToPreviousEdge).LastOrDefault();
            var mirror = maxAngle.Previous.AngleToPreviousEdge < maxAngle.Previous.Previous.AngleToPreviousEdge;
            
            var upDirection = Vector3.Normalize(maxAngle.Previous.TargetNodePosition - maxAngle.TargetNodePosition);
            var rightDirection = Vector3.Cross(upDirection, Vector3.UnitZ);
            var mirrorFactor = mirror ? -1 : 1;
            upDirection = mirrorFactor * upDirection;
            var sideDirection = Vector3.Normalize(rightDirection - upDirection);

            var targetBoundingBox = GetBoundingBoxSize(RoadTextureType.AsymmetricYCrossing, template);

            var lengthToTop = 0.2f * targetBoundingBox.Height;
            var lengthToBottom = 0.8f * targetBoundingBox.Height;
            var halfRoadWidth = template.RoadWidth * template.RoadWidthInTexture / 2f;
            var lengthToLeft = halfRoadWidth;
            var lengthToRight = targetBoundingBox.Width - lengthToLeft;
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

            var crossingSegment = new CrossingRoadSegment(crossingPosition, new[] { top, bottom, side }, start, end, RoadTextureType.AsymmetricYCrossing);

            var topEdge = mirror ? maxAngle.TopologyEdge : maxAngle.Previous.TopologyEdge;
            var bottomEdge = mirror ? maxAngle.Previous.TopologyEdge : maxAngle.TopologyEdge;

            Connect(crossingSegment, topEdge, top, upDirection, 0.04f * template.RoadWidth, edgeSegments);
            Connect(crossingSegment, maxAngle.Previous.Previous.TopologyEdge, side, sideDirection, 0, edgeSegments);
            Connect(crossingSegment, bottomEdge, bottom, -upDirection, 0.055f * template.RoadWidth, edgeSegments);

            crossingSegment.MirrorTexture = mirror;

            return crossingSegment;
        }
        public static CrossingRoadSegment CreateXCrossing(IEnumerable<IncomingRoadData> roads, Vector3 crossingPosition, RoadTemplate template, IDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var roadBottom = roads.FirstOrDefault();
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
            
            var roadWidth = template.RoadWidth * template.RoadWidthInTexture;

            var targetBoundingBox = GetBoundingBoxSize(RoadTextureType.XCrossing, template);

            var top = new RoadSegmentEndPoint(crossingPosition + targetBoundingBox.Height / 2 * upDirection);
            var bottom = new RoadSegmentEndPoint(crossingPosition - targetBoundingBox.Height / 2 * upDirection);
            var right = new RoadSegmentEndPoint(crossingPosition + targetBoundingBox.Height / 2 * rightDirection);
            var left = new RoadSegmentEndPoint(crossingPosition - targetBoundingBox.Height / 2 * rightDirection);

            var start = left.Position;
            var end = right.Position;

            var crossingSegment = new CrossingRoadSegment(crossingPosition, new[] { top, bottom, right, left }, start, end, RoadTextureType.XCrossing);

            var overlap = 0.015f * template.RoadWidth;
            Connect(crossingSegment, roadTop.TopologyEdge, top, upDirection, overlap, edgeSegments);
            Connect(crossingSegment, roadRight.TopologyEdge, right, rightDirection, overlap, edgeSegments);
            Connect(crossingSegment, roadBottom.TopologyEdge, bottom, -upDirection, overlap, edgeSegments);
            Connect(crossingSegment, roadLeft.TopologyEdge, left, -rightDirection, overlap, edgeSegments);

            return crossingSegment;
        }

        private static void Connect(CrossingRoadSegment newSegment, RoadTopologyEdge edge, RoadSegmentEndPoint endPoint, in Vector3 direction, float overlap, IDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
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

        static RectangleF GetBoundingBoxSize(RoadTextureType type, RoadTemplate template)
        {
            var stubLength = 0.5f * (1f - template.RoadWidthInTexture);
            var overlapLength = 0.015f;

            float width, height;

            switch (type)
            {
                case RoadTextureType.AsymmetricYCrossing:
                    width = 1.2f + template.RoadWidthInTexture / 2f;
                    height = 1.33f + overlapLength;
                    break;
                case RoadTextureType.TCrossing:
                    width = template.RoadWidthInTexture + stubLength + overlapLength;
                    height = template.RoadWidthInTexture + 2 * stubLength + 2 * overlapLength;
                    break;
                case RoadTextureType.XCrossing:
                    width = template.RoadWidthInTexture + 2* stubLength + 2* overlapLength;
                    height = template.RoadWidthInTexture + 2 * stubLength + 2 * overlapLength;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown RoadTextureType: " + type);
            }

            return new RectangleF(0, 0, width * template.RoadWidth, height * template.RoadWidth);
        }

        public void GenerateMesh(RoadTemplate template, HeightMap heightMap, List<RoadShaderResources.RoadVertex> vertices, List<ushort> indices)
        {
            var targetBoundingBox = GetBoundingBoxSize(Type, template);
            var halfHeight = targetBoundingBox.Height / 2;

            var mesher = new RoadCrossingMesher(this, halfHeight, template);
            mesher.GenerateMesh(heightMap, vertices, indices);
        }
    }
}
