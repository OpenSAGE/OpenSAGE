using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    enum CrossingType
    {
        T,
        SymmetricY,
        AsymmetricY
    }

    internal sealed class MapTexturePoint
    {
        public Vector3 Position { get; set; }
        public Vector2 TextureCoordinates { get; set; }
    }

    internal sealed class CrossingRoadSegment : IRoadSegment
    {
        public CrossingRoadSegment(IEnumerable<RoadSegmentEndPoint> endPoints, Vector3 start, Vector3 end, CrossingType type)
        {
            EndPoints = endPoints;
            Start = start;
            End = end;
            Type = type;
        }

        private Vector3 Start { get; }
        private Vector3 End { get; }
        public IEnumerable<RoadSegmentEndPoint> EndPoints { get; }
        public CrossingType Type { get; }

        public void GenerateMesh(RoadTemplate template, HeightMap heightMap, List<RoadShaderResources.RoadVertex> vertices, List<ushort> indices)
        {
            const float heightBias = 1f;
            const float createNewVerticesHeightDeltaThreshold = 0.002f;

            //var rightToPosition = Position - Right.Position;
            //var startPosition = Position + rightToPosition;
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
            var centerToEdgeDirection = Vector3.Cross(Vector3.UnitZ, direction);
            var up = Vector3.Cross(direction, centerToEdgeDirection);
            
            var textureAtlasHalfRoadWidth = 0.25f / 2f * template.RoadWidthInTexture * crossingWidthFactor;
            var halfWidth = template.RoadWidth / 2 * crossingWidthFactor;

            var sections = Math.Max(1, (int) (distance / 10));
            var distancePerSection = distance / sections;

            var initialVertexCount = vertices.Count;

            void AddVertexPair(in Vector3 position, float distanceAlongRoad)
            {
                var u = MathUtility.Lerp(uStart, uEnd, distanceAlongRoad / distance);

                var p0 = position - centerToEdgeDirection * halfWidth;
                p0.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p0,
                    Normal = up,
                    UV = new Vector2(u, v - textureAtlasHalfRoadWidth)
                });

                var p1 = position + centerToEdgeDirection * halfWidth;
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
