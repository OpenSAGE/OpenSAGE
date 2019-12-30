using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    internal enum CrossingType
    {
        T,
        SymmetricY,
        AsymmetricY
    }

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
        private CrossingRoadSegment(Vector3 position, IEnumerable<RoadSegmentEndPoint> endPoints, Vector3 start, Vector3 end, CrossingType type)
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
        public CrossingType Type { get; }



        public static CrossingRoadSegment CreateTCrossing(IEnumerable<IncomingRoadData> roads, Vector3 crossingPosition, float templateHalfWidth, IDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var maxAngle = roads.OrderBy(road => road.AngleToPreviousEdge).LastOrDefault();
            var upDirection = Vector3.Normalize(maxAngle.Previous.TargetNodePosition - maxAngle.TargetNodePosition);
            var rightDirection = Vector3.Cross(upDirection, Vector3.UnitZ);

            var crossingWidthFactor = 1; //todo: take from lookup
            var halfWidth = templateHalfWidth * crossingWidthFactor;

            var top = new RoadSegmentEndPoint(crossingPosition + halfWidth * upDirection);
            var bottom = new RoadSegmentEndPoint(crossingPosition - halfWidth * upDirection);
            var right = new RoadSegmentEndPoint(crossingPosition + halfWidth * rightDirection);

            var start = crossingPosition - halfWidth * rightDirection;
            var end = right.Position;

            var crossingSegment = new CrossingRoadSegment(crossingPosition, new[] { top, bottom, right }, start, end, CrossingType.T);

            Connect(crossingSegment, maxAngle.Previous.TopologyEdge, top, upDirection, edgeSegments);
            Connect(crossingSegment, maxAngle.Previous.Previous.TopologyEdge, right, rightDirection, edgeSegments);
            Connect(crossingSegment, maxAngle.TopologyEdge, bottom, -upDirection, edgeSegments);

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

        //will be used for y crossings
        private (Vector3 start, Vector3 end) CreateStartAndEndAlongDirection(IEnumerable<Vector3> endpoints, Vector3 normalizedDirection, Vector3 crossingPosition)
        {
            var orthDirection = Vector3.Cross(normalizedDirection, Vector3.UnitZ);

            var valuesAlongDirection = endpoints.Select(end => Vector3.Dot(end - crossingPosition, normalizedDirection));
            var valuesOrthToDirection = endpoints.Select(end => Vector3.Dot(end - crossingPosition, orthDirection));

            var minAlongDirection = valuesAlongDirection.Min();
            var maxAlongDirection = valuesAlongDirection.Max();
            var middleOrthToDirection = (valuesOrthToDirection.Min() + valuesOrthToDirection.Max()) / 2;

            var startPoint = crossingPosition + minAlongDirection * normalizedDirection + middleOrthToDirection * orthDirection;
            var endPoint = crossingPosition + maxAlongDirection * normalizedDirection + middleOrthToDirection * orthDirection;

            return (startPoint, endPoint);
        }



        public void GenerateMesh(RoadTemplate template, HeightMap heightMap, List<RoadShaderResources.RoadVertex> vertices, List<ushort> indices)
        {
            const float heightBias = 1f;
            const float createNewVerticesHeightDeltaThreshold = 0.002f;

            var startPosition = Start;
            startPosition.Z = heightMap.GetHeight(startPosition.X, startPosition.Y);

            var endPosition = End;
            endPosition.Z = heightMap.GetHeight(endPosition.X, endPosition.Y);

            //TODO: create a lookup of these values for each of the crossing types. For now: use those for t-crossing
            var crossingWidthFactor = 1;
            var uStart = 0.71f;
            var uEnd = 0.96f;
            var v = 0.5f;

            var distance = Vector3.Distance(Start, End);
            var direction = Vector3.Normalize(End - Start);
            var directionNormal = Vector3.Cross(Vector3.UnitZ, direction);
            var up = Vector3.UnitZ;
            
            var textureAtlasHalfRoadWidth = 0.25f / 2f * template.RoadWidthInTexture * crossingWidthFactor;
            var halfWidth = template.RoadWidth / 2 * crossingWidthFactor;

            var sections = Math.Max(1, (int) (distance / 10));
            var distancePerSection = distance / sections;

            var initialVertexCount = vertices.Count;

            void AddVertexPair(in Vector3 position, float distanceAlongRoad)
            {
                var u = MathUtility.Lerp(uStart, uEnd, distanceAlongRoad / distance);

                var p0 = position - directionNormal * halfWidth;
                p0.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p0,
                    Normal = up,
                    UV = new Vector2(u, v - textureAtlasHalfRoadWidth)
                });

                var p1 = position + directionNormal * halfWidth;
                p1.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p1,
                    Normal = up,
                    UV = new Vector2(u, v + textureAtlasHalfRoadWidth)
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

            // Add last chunk.
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
