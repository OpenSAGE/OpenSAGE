using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Terrain.Roads
{
    internal class InsertPosition
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public float relativeProgress { get; set; }
        public float distanceAlongRoad { get; set; }
    }


    internal abstract class RoadSegmentMesher
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

            // vertical texture coordinate
            var vStart = TextureBounds.TopLeft.Y;
            var vEnd = TextureBounds.BottomLeft.Y;
            var v = (vEnd + vStart) / 2;
            var textureAtlasHalfHeight = (vEnd - vStart) / 2;
            var vOffset = Segment.MirrorTexture ? -textureAtlasHalfHeight : textureAtlasHalfHeight;

            // let the derived classes do some initializations
            Prepare();

            // remember which vercies were already there
            var initialVertexCount = vertices.Count;            

            // for a given spot along rendering the road, generate the border vertices on both sides of the road
            void AddVertexPair(InsertPosition insertPos)
            {
                var position = insertPos.Position;

                // let the derived classes do some decisions
                var toBorder = ToTopBorder(insertPos.relativeProgress);
                var uOffset = TopUOffset(insertPos.relativeProgress);
                var u = GetU(insertPos.relativeProgress, insertPos.distanceAlongRoad);

                // generate border positions and determine their z coordinate
                var p0 = position - toBorder;
                var p1 = position + toBorder;
                
                var sideZ = Math.Max(heightMap.GetHeight(p0.X, p0.Y), heightMap.GetHeight(p1.X, p1.Y));
                var z = Math.Max(position.Z, sideZ) + heightBias;
                p0.Z = z;
                p1.Z = z;                
                
                // add the border vertices                
                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p0,
                    Normal = insertPos.Normal,
                    UV = new Vector2(u - uOffset, v - vOffset)
                });
                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = p1,
                    Normal = insertPos.Normal,
                    UV = new Vector2(u + uOffset, v + vOffset)
                });
            }

            var insertPositions = GenerateInsertPositions(heightMap);
            foreach(var insertPosition in insertPositions)
            {
                AddVertexPair(insertPosition);
            }
            
            // generate triangles for the inserted vertices
            GenerateTriangles(initialVertexCount, vertices, indices);
        }



        protected virtual void Prepare() { }
        protected abstract Vector3 ToTopBorder(float relativeProgress);
        protected abstract float GetU(float relativeProgress, float distanceAlongRoad);
        protected abstract float TopUOffset(float relativeProgress);

        private IList<InsertPosition> GenerateInsertPositions(HeightMap heightMap)
        {
            // computations including the z coordinate
            // -> only for generating the mesh triangles, not for any angle computations / crossings with z axis / ... !
            var startWithZ = Segment.StartPosition.WithZ(heightMap.GetHeight(Segment.StartPosition.X, Segment.StartPosition.Y));
            var endWithZ = Segment.EndPosition.WithZ(heightMap.GetHeight(Segment.EndPosition.X, Segment.EndPosition.Y));
            var segmentVector = endWithZ - startWithZ;
            var distanceWithZ = Vector3.Distance(startWithZ, endWithZ);

            var sectionCount = Math.Max(1, (int) (distanceWithZ / 10));

            // generate candidate positions for cutting the road segment
            var positionCandidates = new List<InsertPosition>();
            for (int i = 0; i <= sectionCount; ++i)
            {
                var relativeProgress = (float) i / (float) sectionCount;
                var position = startWithZ + relativeProgress * segmentVector;
                positionCandidates.Add(new InsertPosition()
                {
                    Position = position.WithZ(heightMap.GetUpperHeight(position.X, position.Y)),
                    relativeProgress = relativeProgress
                });
            }

            // choose which of those to use (don't need additional triangles where the road segment is unbumpys)
            const float createNewVerticesHeightDeltaThreshold = 0.002f;
            var usefulPositions = new List<InsertPosition>();
            usefulPositions.Add(positionCandidates[0]);
            for (int i = 1; i < sectionCount; ++i)
            {
                var interpolatedHeight = (positionCandidates[i - 1].Position.Z + positionCandidates[i + 1].Position.Z) / 2;
                if (Math.Abs(interpolatedHeight - positionCandidates[i].Position.Z) > createNewVerticesHeightDeltaThreshold)
                {
                    usefulPositions.Add(positionCandidates[i]);
                }
            }
            usefulPositions.Add(positionCandidates[sectionCount]);

            // get vertex normals
            var count = usefulPositions.Count;
            for (int i = 0; i < count; ++i)
            {
                var prevRoadSection = i > 0 ? Vector3.Normalize(usefulPositions[i].Position - usefulPositions[i - 1].Position) : Vector3.Zero;
                var nextRoadSection = i < count - 1 ? Vector3.Normalize(usefulPositions[i + 1].Position - usefulPositions[i].Position) : Vector3.Zero;

                var avgRoadDirection = Vector3.Normalize(prevRoadSection + nextRoadSection);
                usefulPositions[i].Normal = Vector3.Cross(DirectionNormalNoZ, avgRoadDirection);
            }

            // get distance along road
            usefulPositions[0].distanceAlongRoad = 0;
            for (int i = 1; i<count; ++i)
            {
                var length = Vector3.Distance(usefulPositions[i].Position, usefulPositions[i - 1].Position);
                usefulPositions[i].distanceAlongRoad = usefulPositions[i - 1].distanceAlongRoad + length;
            }

            return usefulPositions.ToList();
        }

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

    internal sealed class CrossingRoadSegmentMesher : RoadSegmentMesher
    {
        public CrossingRoadSegmentMesher(IRoadSegment segment, float halfHeight, RoadTemplate template)
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
            //for crossing: texture should stretch to the base mesh -> interpolate the boundary values
            return MathUtility.Lerp(TextureBounds.BottomLeft.X, TextureBounds.BottomRight.X, relativeProgress);
        }
    }

    internal sealed class StraightRoadSegmentMesher : RoadSegmentMesher
    {
        public StraightRoadSegmentMesher(IRoadSegment segment, float halfHeight, RoadTemplate template)
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
                throw new InvalidOperationException();
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
            // for roads: the road segment we are meshing may be much longer or shorter than the texture road length,
            // so stretching the texture on each segment would look odd -> repeat it instead
            return distanceAlongRoad / TextureRoadLength;
        }

        /// <summary>
        /// Generate vector from the start/end position of an edge to the corner of the mesh's base geometry
        /// </summary>
        /// <param name="neighbor"></param>
        /// <param name="atEnd"></param>
        /// <returns></returns>
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

            // When two road segments meet in an angled curve, their meeting edge is tilted and thus longer than the width of the road
            // -> divide by cosine
            // For straight roads ending in a crossing:
            // -> the angles in the crossing texture may not well match the actual angles of the incoming roads
            //    (especially for x-crossings, since there's only one texture for 4-road crossings which assumes 90° everywhere)
            // -> the meeting edge may become quite tilted and noticeably longer than road width,
            //    while the road shown in the texture of the crossing always has the fixed road width
            // -> to avoid visible breaks between the road segment and the crossing texture, distort the edge so its tilted seam is 'roadwidth' long
            var cosine = Vector3.Dot(DirectionNormalNoZ, toCornerDirection);
            var toCornerLength = neighbor.To is CrossingRoadSegment ? HalfHeight : HalfHeight / cosine;
            return toCornerDirection * toCornerLength;
        }
    }
}
