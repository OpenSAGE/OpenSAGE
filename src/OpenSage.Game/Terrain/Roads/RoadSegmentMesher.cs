using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    abstract class RoadSegmentMesher
    {
        protected RoadSegmentMesher(IRoadSegment segment, float halfHeight, RoadTemplate template)
        {
            Segment = segment;
            HalfHeight = halfHeight;
            Template = template;

            var startNoZ = Segment.StartPosition.WithZ(0);
            var endNoZ = Segment.EndPosition.WithZ(0);
            DirectionNoZ = Vector3.Normalize(endNoZ - startNoZ);
            DirectionNormalNoZ = Vector3.Cross(DirectionNoZ, Vector3.UnitZ);
            
            TextureBounds = TextureAtlas.ForRoadWidth(Template.RoadWidthInTexture)[Segment.Type];
        }

        protected IRoadSegment Segment { get; }
        protected float HalfHeight { get; }
        protected RoadTemplate Template { get; }
        protected Vector3 DirectionNoZ { get; }
        protected Vector3 DirectionNormalNoZ { get; }
        protected TextureCoordinates TextureBounds { get; }


        public void GenerateMesh(HeightMap heightMap, List<RoadShaderResources.RoadVertex> vertices, List<ushort> indices)
        {
            const float heightBias = 0.1f;
            const float createNewVerticesHeightDeltaThreshold = 0.002f;

            var up = Vector3.UnitZ;

            var startWithZ = Segment.StartPosition.WithZ(heightMap.GetHeight(Segment.StartPosition.X, Segment.StartPosition.Y));
            var endWithZ = Segment.EndPosition.WithZ(heightMap.GetHeight(Segment.EndPosition.X, Segment.EndPosition.Y));
            var directionWithZ = Vector3.Normalize(endWithZ - startWithZ);
            var distance = Vector3.Distance(startWithZ, endWithZ);

            var vStart = TextureBounds.TopLeft.Y;
            var vEnd = TextureBounds.BottomLeft.Y;
            var v = (vEnd + vStart) / 2;
            var textureAtlasHalfHeight = (vEnd - vStart) / 2;
            var vOffset = Segment.MirrorTexture ? -textureAtlasHalfHeight : textureAtlasHalfHeight;

            Prepare();

            var sections = Math.Max(1, (int) (distance / 10));
            var distancePerSection = distance / sections;

            var initialVertexCount = vertices.Count;            

            void AddVertexPair(in Vector3 position, float distanceAlongRoad)
            {
                var relativeProgress = distanceAlongRoad / distance;
                var toBorder = ToTopBorder(relativeProgress);
                var uOffset = TopUOffset(relativeProgress);
                var u = GetU(relativeProgress, distanceAlongRoad);

                var p0 = position - toBorder;
                p0.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p0,
                    Normal = up,
                    UV = new Vector2(u - uOffset, v - vOffset)
                });

                var p1 = position + toBorder;
                p1.Z += heightBias;

                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p1,
                    Normal = up,
                    UV = new Vector2(u + uOffset, v + vOffset)
                });
            }

            AddVertexPair(startWithZ, 0);

            var previousPoint = startWithZ;
            var previousPointDistance = 0f;

            for (int i = 1; i < sections; i++)
            {
                var currentDistance = i * distancePerSection;
                var position = startWithZ + directionWithZ * currentDistance;
                var actualHeight = heightMap.GetHeight(position.X, position.Y);
                var interpolatedHeight = MathUtility.Lerp(previousPoint.Z, endWithZ.Z, (currentDistance - previousPointDistance) / distance);

                if (Math.Abs(actualHeight - interpolatedHeight) > createNewVerticesHeightDeltaThreshold)
                {
                    AddVertexPair(position, currentDistance);
                    previousPoint = position;
                    previousPointDistance = currentDistance;
                }
            }

            AddVertexPair(endWithZ, distance);

            GenerateTriangles(initialVertexCount, vertices, indices);
        }

        protected virtual void Prepare() { }
        protected abstract Vector3 ToTopBorder(float relativeProgress);
        protected abstract float GetU(float relativeProgress, float distanceAlongRoad);
        protected abstract float TopUOffset(float relativeProgress);

        private void GenerateTriangles(int initialVertexCount, List<RoadShaderResources.RoadVertex> vertices, List<ushort> indices)
        {
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

    class RoadCrossingMesher : RoadSegmentMesher
    {
        public RoadCrossingMesher(IRoadSegment segment, float halfHeight, RoadTemplate template)
            : base(segment, halfHeight, template)
        {
        }

        protected override Vector3 ToTopBorder(float relativeProgress)
        {
            return HalfHeight * DirectionNormalNoZ;
        }
        protected override float TopUOffset(float relativeProgress)
        {
            return 0;
        }

        protected override float GetU(float relativeProgress, float _)
        {
            return MathUtility.Lerp(TextureBounds.BottomLeft.X, TextureBounds.BottomRight.X, relativeProgress);
        }
    }

    class StraightRoadMesher : RoadSegmentMesher
    {
        public StraightRoadMesher(IRoadSegment segment, float halfHeight, RoadTemplate template)
            : base(segment, halfHeight, template)
        {
        }
        private Vector3 StartToTopCorner { get; set; }
        private Vector3 EndToTopCorner { get; set; }
        private float StartTopUOffset { get; set; }
        private float EndTopUOffset { get; set; }
        private float TextureRoadLength { get; set; }

        protected override void Prepare()
        {
            var straightRoad = Segment as StraightRoadSegment;
            if (straightRoad == null)
            {
                throw new ArgumentException();
            }
            StartToTopCorner = ToCorner(straightRoad.Start, false);
            EndToTopCorner = ToCorner(straightRoad.End, true);
            TextureRoadLength = Template.RoadWidth * 4;
            StartTopUOffset = Vector3.Dot(StartToTopCorner, DirectionNoZ) / TextureRoadLength;
            EndTopUOffset = Vector3.Dot(EndToTopCorner, DirectionNoZ) / TextureRoadLength;
        }

        protected override float TopUOffset(float relativeProgress)
        {
            return MathUtility.Lerp(StartTopUOffset, EndTopUOffset, relativeProgress);
        }

        protected override Vector3 ToTopBorder(float relativeProgress)
        {
            return Vector3.Lerp(StartToTopCorner, EndToTopCorner, relativeProgress);
        }
        protected override float GetU(float _, float distanceAlongRoad)
        {
            return distanceAlongRoad / TextureRoadLength;
        }

        private Vector3 ToCorner(RoadSegmentEndPoint neighbor, bool atEnd)
        {
            var neighborDirection = (atEnd ? -1 : 1) * neighbor?.IncomingDirection ?? Vector3.Zero;
            var neighborNormal = Vector3.Cross(Vector3.Normalize(neighborDirection.WithZ(0)), Vector3.UnitZ);
            var toCornerDirection = neighbor.To switch
            {
                null => DirectionNormalNoZ,                          // if I have no neighbor, use my own normal
                CrossingRoadSegment _ => neighborNormal,             // if my neighbor is an unflexible crossing, use its normal
                _ => (DirectionNormalNoZ + neighborNormal) / 2,      // otherwise, meet in the middle
            };
            var cosine = Vector3.Dot(DirectionNormalNoZ, toCornerDirection);
            var toCornerLength = HalfHeight / cosine;
            return toCornerDirection * toCornerLength;
        }
    }
}
