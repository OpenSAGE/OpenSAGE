using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class StraightRoadSegment : IRoadSegment
    {
        public RoadSegmentEndPoint Start { get; }
        public RoadSegmentEndPoint End { get; }

        public StraightRoadSegment(in Vector3 start, in Vector3 end)
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

        public void GenerateMesh(
            RoadTemplate template,
            HeightMap heightMap,
            List<RoadShaderResources.RoadVertex> vertices,
            List<ushort> indices)
        {
            const float heightBias = 1f;
            // TODO consider height while creating vertices
            const float createNewVerticesHeightDeltaThreshold = 0.002f;
            const float textureAtlasRoadCenter = 1f / 6f;
            var halfWidth = template.RoadWidth / 2f;
            var textureAtlasHalfRoadWidth = 0.25f / 2f * template.RoadWidthInTexture;

            var startPosition = Start.Position.WithZ(heightMap.GetHeight(Start.Position.X, Start.Position.Y));
            var endPosition = End.Position.WithZ(heightMap.GetHeight(End.Position.X, End.Position.Y));

            var distance = Vector3.Distance(startPosition, endPosition);
            var direction = (endPosition - startPosition) / distance;
            var directionNormal = Vector3.Cross(Vector3.UnitZ, direction);

            Vector3 ToCorner(in Vector3 normal1, in Vector3 normal2)
            {
                var toCornerDirection = (normal1 + normal2) / 2;
                var toCornerLength = halfWidth / Vector3.Dot(directionNormal, toCornerDirection);
                return toCornerDirection * toCornerLength;
            }

            var startIncomingDirectionNormal = Start.To == null ? directionNormal : Vector3.Cross(Vector3.UnitZ, Vector3.Normalize(Start.IncomingDirection));
            var startToCorner = ToCorner(directionNormal, startIncomingDirectionNormal);

            var endIncomingDirectionNormal = End.To == null ? directionNormal : Vector3.Cross(Vector3.UnitZ, -Vector3.Normalize(End.IncomingDirection));
            var endToCorner = ToCorner(directionNormal, endIncomingDirectionNormal);

            var sections = Math.Max(1, (int)(distance / 10));
            var distancePerSection = distance / sections;
            var initialVertexCount = vertices.Count;

            AddVertexPair(startPosition, startToCorner, 0);

            for (int i = 1; i < sections; i++)
            {
                var distanceSoFar = i * distancePerSection;
                var position = startPosition + direction * distanceSoFar;
                var positionToSector = Vector3.Lerp(startToCorner, endToCorner, distanceSoFar / distance);

                AddVertexPair(position, positionToSector, distanceSoFar);
            }

            AddVertexPair(endPosition, endToCorner, distance);

            for (var i = initialVertexCount; i < vertices.Count - 2; i += 2)
            {
                indices.Add((ushort) (i + 0));
                indices.Add((ushort) (i + 1));
                indices.Add((ushort) (i + 2));

                indices.Add((ushort) (i + 1));
                indices.Add((ushort) (i + 2));
                indices.Add((ushort) (i + 3));
            }
            
            void AddVertexPair(in Vector3 position, in Vector3 toBorder, float distanceAlongRoad)
            {
                // TODO calculate up based on elevation
                var up = Vector3.Cross(direction, toBorder);

                var u = distanceAlongRoad / 140;

                var p0 = position - toBorder;
                p0.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p0,
                    Normal = up,
                    UV = new Vector2(u, textureAtlasRoadCenter - textureAtlasHalfRoadWidth)
                });

                var p1 = position + toBorder;
                p1.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p1,
                    Normal = up,
                    UV = new Vector2(u, textureAtlasRoadCenter + textureAtlasHalfRoadWidth)
                });
            }
        }
    }
}
