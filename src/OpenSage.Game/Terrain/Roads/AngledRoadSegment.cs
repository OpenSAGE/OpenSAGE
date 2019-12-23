using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class AngledRoadSegment : IRoadSegment
    {
        private IEnumerable<Vector3> _intermediatePositions;

        public RoadSegmentEndPoint Start { get; }
        public RoadSegmentEndPoint End { get; }

        public AngledRoadSegment(in Vector3 start, in Vector3 end, IEnumerable<Vector3> intermediatePositions = null)
        {
            Start = new RoadSegmentEndPoint(start);
            End = new RoadSegmentEndPoint(end);
            _intermediatePositions = intermediatePositions ?? Enumerable.Empty<Vector3>();
        }

        public IEnumerable<RoadSegmentEndPoint> EndPoints
        {
            get
            {
                yield return Start;
                yield return End;
            }
        }

        public void GenerateMesh(
            RoadTemplate template,
            HeightMap heightMap,
            List<RoadShaderResources.RoadVertex> vertices,
            List<ushort> indices)
        {
            const float heightBias = 1f;
            const float createNewVerticesHeightDeltaThreshold = 0.002f;

            var startPosition = new Vector3(
                Start.Position.X,
                Start.Position.Y,
                heightMap.GetHeight(Start.Position.X, Start.Position.Y));

            var endPosition = new Vector3(
                End.Position.X,
                End.Position.Y,
                heightMap.GetHeight(End.Position.X, End.Position.Y));

            var distance = Vector3.Distance(startPosition, endPosition);
            var direction = Vector3.Normalize(endPosition - startPosition);
            var centerToEdgeDirection = Vector3.Cross(Vector3.UnitZ, direction);
            var up = Vector3.Cross(direction, centerToEdgeDirection);

            var halfWidth = template.RoadWidth / 2;

            var textureAtlasSplit = 1 / 3f;

            // Step along road segment in units of 10. If the delta between
            // (a) the straight line from previous point to finish and
            // (b) the actual height of the terrain at this point
            // is > a threshold, create extra vertices.
            // TODO: I don't know if this is the right algorithm.

            var initialVertexCount = vertices.Count;

            void AddVertexPair(in Vector3 position, float distanceAlongRoad)
            {
                var u = distanceAlongRoad / 50;

                var p0 = position - centerToEdgeDirection * halfWidth;
                p0.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p0,
                    Normal = up,
                    UV = new Vector2(u, 0)
                });

                var p1 = position + centerToEdgeDirection * halfWidth;
                p1.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p1,
                    Normal = up,
                    UV = new Vector2(u, textureAtlasSplit)
                });
            }

            AddVertexPair(startPosition, 0);

            var previousPoint = startPosition;
            var previousPointDistance = 0;

            for (var currentDistance = 10; currentDistance < distance; currentDistance += 10)
            {
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
