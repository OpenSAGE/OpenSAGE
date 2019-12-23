using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class TRoadSegment : IRoadSegment
    {
        public TRoadSegment(Vector3 position, RoadSegmentEndPoint top, RoadSegmentEndPoint right, RoadSegmentEndPoint bottom)
        {
            Position = position;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Vector3 Position { get; }
        public RoadSegmentEndPoint Top { get; }
        public RoadSegmentEndPoint Right { get; }
        public RoadSegmentEndPoint Bottom { get; }

        public IEnumerable<RoadSegmentEndPoint> EndPoints
        {
            get
            {
                yield return Top;
                yield return Right;
                yield return Bottom;
            }
        }

        public void GenerateMesh(RoadTemplate template, HeightMap heightMap, List<RoadShaderResources.RoadVertex> vertices, List<ushort> indices)
        {
            const float heightBias = 1f;
            const float createNewVerticesHeightDeltaThreshold = 0.002f;

            var rightToPosition = Position - Right.Position;
            var startPosition = Position + rightToPosition;
            startPosition.Z = heightMap.GetHeight(startPosition.X, startPosition.Y);

            var endPosition = new Vector3(
                Right.Position.X,
                Right.Position.Y,
                heightMap.GetHeight(Right.Position.X, Right.Position.Y));

            var distance = Vector3.Distance(startPosition, endPosition);
            var direction = Vector3.Normalize(rightToPosition);
            var centerToEdgeDirection = Vector3.Cross(Vector3.UnitZ, direction);
            var up = Vector3.Cross(direction, centerToEdgeDirection);

            var halfWidth = template.RoadWidth / 2;

            var initialVertexCount = vertices.Count;

            void AddVertexPair(in Vector3 position, float distanceAlongRoad)
            {
                var u = MathUtility.Lerp(0.71f, 0.96f, distanceAlongRoad / template.RoadWidth);

                var p0 = position - centerToEdgeDirection * halfWidth;
                p0.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p0,
                    Normal = up,
                    UV = new Vector2(u, 0.37f)
                });

                var p1 = position + centerToEdgeDirection * halfWidth;
                p1.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p1,
                    Normal = up,
                    UV = new Vector2(u, 0.62f)
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
