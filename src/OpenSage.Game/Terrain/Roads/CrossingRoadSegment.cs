using System;
using System.Collections.Generic;
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
            Start = start;
            End = end;
            Type = type;
        }

        private Vector3 Start { get; }
        private Vector3 End { get; }
        private Vector3 Position { get; }
        public IEnumerable<RoadSegmentEndPoint> EndPoints { get; }
        public RoadTextureType Type { get; }
        public bool Mirror { get; set; } = false;



        public static CrossingRoadSegment CreateTCrossing(IEnumerable<IncomingRoadData> roads, Vector3 crossingPosition, RoadTemplate template, IDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var maxAngle = roads.OrderBy(road => road.AngleToPreviousEdge).LastOrDefault();
            var upDirection = Vector3.Normalize(maxAngle.Previous.TargetNodePosition - maxAngle.TargetNodePosition);
            var rightDirection = Vector3.Cross(upDirection, Vector3.UnitZ);

            var roadWidth = template.RoadWidth * template.RoadWidthInTexture;
            var halfRoadWidth = roadWidth / 2f;

            var targetBoundingBox = GetBoundingBox(RoadTextureType.TCrossing, template);

            var top = new RoadSegmentEndPoint(crossingPosition + targetBoundingBox.Height / 2 * upDirection);
            var bottom = new RoadSegmentEndPoint(crossingPosition - targetBoundingBox.Height / 2 * upDirection);
            var right = new RoadSegmentEndPoint(crossingPosition + (targetBoundingBox.Width - halfRoadWidth) * rightDirection);

            var start = crossingPosition - halfRoadWidth * rightDirection;
            var end = right.Position;

            var crossingSegment = new CrossingRoadSegment(crossingPosition, new[] { top, bottom, right }, start, end, RoadTextureType.TCrossing);

            Connect(crossingSegment, maxAngle.Previous.TopologyEdge, top, upDirection, edgeSegments);
            Connect(crossingSegment, maxAngle.Previous.Previous.TopologyEdge, right, rightDirection, edgeSegments);
            Connect(crossingSegment, maxAngle.TopologyEdge, bottom, -upDirection, edgeSegments);

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

            var targetBoundingBox = GetBoundingBox(RoadTextureType.AsymmetricYCrossing, template);

            var lengthToTop = 0.2f * targetBoundingBox.Height;
            var lengthToBottom = 0.8f * targetBoundingBox.Height;
            var halfRoadWidth = template.RoadWidth * template.RoadWidthInTexture / 2f;
            var lengthToLeft = halfRoadWidth;
            var lengthToRight = targetBoundingBox.Width - lengthToLeft;
            var lengthToSide = template.RoadWidth;

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

            Connect(crossingSegment, topEdge, top, upDirection, edgeSegments);
            Connect(crossingSegment, maxAngle.Previous.Previous.TopologyEdge, side, sideDirection, edgeSegments);
            Connect(crossingSegment, bottomEdge, bottom, -upDirection, edgeSegments);

            crossingSegment.Mirror = mirror;

            return crossingSegment;
        }

        private static void Connect(CrossingRoadSegment newSegment, RoadTopologyEdge edge, RoadSegmentEndPoint endPoint, in Vector3 direction, IDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
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

        //will be used for y crossings?
        //private (Vector3 start, Vector3 end) CreateStartAndEndAlongDirection(IEnumerable<Vector3> endpoints, Vector3 normalizedDirection, Vector3 crossingPosition)
        //{
        //    var orthDirection = Vector3.Cross(normalizedDirection, Vector3.UnitZ);

        //    var valuesAlongDirection = endpoints.Select(end => Vector3.Dot(end - crossingPosition, normalizedDirection));
        //    var valuesOrthToDirection = endpoints.Select(end => Vector3.Dot(end - crossingPosition, orthDirection));

        //    var minAlongDirection = valuesAlongDirection.Min();
        //    var maxAlongDirection = valuesAlongDirection.Max();
        //    var middleOrthToDirection = (valuesOrthToDirection.Min() + valuesOrthToDirection.Max()) / 2;

        //    var startPoint = crossingPosition + minAlongDirection * normalizedDirection + middleOrthToDirection * orthDirection;
        //    var endPoint = crossingPosition + maxAlongDirection * normalizedDirection + middleOrthToDirection * orthDirection;

        //    return (startPoint, endPoint);
        //}

        static RectangleF GetBoundingBox(RoadTextureType type, RoadTemplate template)
        {
            var stubLength = 0.5f * (1f - template.RoadWidthInTexture);
            var overlayLength = 0.015f;

            float width, height;

            switch(type)
            {
                case RoadTextureType.AsymmetricYCrossing:
                    width = 1.2f + template.RoadWidthInTexture / 2f;
                    height = 1.33f + overlayLength;
                    break;
                case RoadTextureType.TCrossing:
                    width = template.RoadWidthInTexture + stubLength + overlayLength;
                    height = template.RoadWidthInTexture + 2 * stubLength + 2 * overlayLength;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown RoadTextureType: " + type);
            }

            return new RectangleF(0, 0, width * template.RoadWidth, height * template.RoadWidth);
        }


        public void GenerateMesh(RoadTemplate template, HeightMap heightMap, List<RoadShaderResources.RoadVertex> vertices, List<ushort> indices)
        {
            const float heightBias = 0.1f;
            const float createNewVerticesHeightDeltaThreshold = 0.002f;
            
            var distance = Vector3.Distance(Start, End);
            var direction = Vector3.Normalize(End - Start);
            var directionNormal = Vector3.Cross(direction, Vector3.UnitZ);
            var up = Vector3.UnitZ;

            var startPosition = Start;
            startPosition.Z = heightMap.GetHeight(startPosition.X, startPosition.Y);

            var endPosition = End;
            endPosition.Z = heightMap.GetHeight(endPosition.X, endPosition.Y);

            var targetBoundingBox = GetBoundingBox(Type, template);
            var halfHeight = targetBoundingBox.Height / 2;

            var textureBounds = TextureAtlas.ForRoadWidth(template.RoadWidthInTexture)[Type];
            var uStart = textureBounds.BottomLeft.X;
            var uEnd = textureBounds.BottomRight.X;
            var vStart = textureBounds.TopLeft.Y;
            var vEnd = textureBounds.BottomLeft.Y;

            var v = (vEnd + vStart) / 2;
            var textureAtlasHalfHeight = (vEnd - vStart) / 2;
            var vOffset = Mirror ? -textureAtlasHalfHeight : textureAtlasHalfHeight;
            
            var sections = Math.Max(1, (int) (distance / 10));
            var distancePerSection = distance / sections;

            var initialVertexCount = vertices.Count;

            void AddVertexPair(in Vector3 position, float distanceAlongRoad)
            {
                var u = MathUtility.Lerp(uStart, uEnd, distanceAlongRoad / distance);

                var p0 = position - directionNormal * halfHeight;
                p0.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p0,
                    Normal = up,
                    UV = new Vector2(u, v - vOffset)
                });

                var p1 = position + directionNormal * halfHeight;
                p1.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p1,
                    Normal = up,
                    UV = new Vector2(u, v + vOffset)
                });
            }

            AddVertexPair(startPosition, 0);

            var previousPoint = startPosition;
            var previousPointDistance = 0f;

            for (int i = 1; i < sections; i++)
            {
                var currentDistance = i * distancePerSection;
                var position = startPosition + direction * currentDistance;
                var actualHeight = heightMap.GetHeight(position.X, position.Y);
                var interpolatedHeight = MathUtility.Lerp(previousPoint.Z, endPosition.Z, (currentDistance - previousPointDistance) / distance);

                if (Math.Abs(actualHeight - interpolatedHeight) > createNewVerticesHeightDeltaThreshold)
                {
                    AddVertexPair(position, currentDistance);
                    previousPoint = position;
                    previousPointDistance = currentDistance;
                }
            }

            AddVertexPair(endPosition, distance);

            for (var i = initialVertexCount; i < vertices.Count - 2; i += 2)
            {
                indices.Add((ushort) (i + 0));
                indices.Add((ushort) (i + 1));
                indices.Add((ushort) (i + 2));

                indices.Add((ushort) (i + 1));
                indices.Add((ushort) (i + 2));
                indices.Add((ushort) (i + 3));
            }
        }
    }
}
