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
        public float RelativeProgress { get; set; }
        public float DistanceAlongRoad { get; set; }
        public Vector3 TopPosition { get; set; }
        public Vector3 BottomPosition { get; set; }
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

        // helper method used by crossings and straight roads
        protected (float midV, float vOffset) GetVFromAxisAlignedBoundingBox()
        {
            var vStart = TextureBounds.TopLeft.Y;
            var vEnd = TextureBounds.BottomLeft.Y;
            var v = (vEnd + vStart) / 2;
            var textureAtlasHalfHeight = (vEnd - vStart) / 2;
            var vOffset = Segment.MirrorTexture ? -textureAtlasHalfHeight : textureAtlasHalfHeight;
            return (v, vOffset);
        }

        //helper method used by crossings and curves
        protected Vector3 GetNeighborNormal(RoadSegmentEndPoint neighbor, bool atEnd)
        {
            var neighborDirection = (atEnd ? -1 : 1) * neighbor?.IncomingDirection ?? Vector3.Zero;
            return Vector3.Cross(Vector3.Normalize(neighborDirection.WithZ(0)), Vector3.UnitZ);            
        }

        public void GenerateMesh(HeightMap heightMap, List<RoadShaderResources.RoadVertex> vertices, List<ushort> indices)
        {
            // let the derived classes do some initializations
            Prepare();

            // remember which vercies were already there
            var initialVertexCount = vertices.Count;            

            // for a given spot along rendering the road, generate the border vertices on both sides of the road
            void AddVertexPair(InsertPosition insertPos)
            {
                var position = insertPos.Position;

                // let the derived classes do some decisions
                var (uvTop, uvBottom) = GetTopBottomTextureCoordinates(insertPos.RelativeProgress, insertPos.DistanceAlongRoad);
                
                // add the border vertices                
                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = insertPos.BottomPosition,
                    Normal = insertPos.Normal,
                    UV = uvBottom
                });
                vertices.Add(new RoadShaderResources.RoadVertex
                {
                    Position = insertPos.TopPosition,
                    Normal = insertPos.Normal,
                    UV = uvTop
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
        protected abstract (Vector2 top, Vector2 bottom) GetTopBottomTextureCoordinates(float relativeProgress, float distanceAlongRoad);

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
                var newPoint = new InsertPosition()
                {
                    Position = position,
                    RelativeProgress = relativeProgress
                };
                UpdateHeights(newPoint, heightMap);
                positionCandidates.Add(newPoint);
            }

            // choose which of those to use (don't need additional triangles where the road segment is unbumpy)
            const float createNewVerticesHeightDeltaThreshold = 0.001f;
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
            usefulPositions[0].DistanceAlongRoad = 0;
            for (int i = 1; i<count; ++i)
            {
                var length = Vector3.Distance(usefulPositions[i].Position, usefulPositions[i - 1].Position);
                usefulPositions[i].DistanceAlongRoad = usefulPositions[i - 1].DistanceAlongRoad + length;
            }

            return usefulPositions.ToList();
        }

        private void UpdateHeights(InsertPosition p, HeightMap heightMap)
        {
            var mid = p.Position;
            var toBorder = ToTopBorder(p.RelativeProgress);

            // find relevant height of points along this cross section of the road
            var sections = Math.Max(1, (int) (toBorder.Length() / 10));
            var maxHeight = heightMap.GetUpperHeight(mid.X, mid.Y);
            var pTop = p.Position;
            var pBottom = p.Position;
            for (int i = 1; i <= sections; ++i)
            {
                var scaledVector = (float) i / sections * toBorder;
                pTop = mid + scaledVector;
                pBottom = mid - scaledVector;
                maxHeight = MathF.Max(maxHeight, heightMap.GetUpperHeight(pTop.X, pTop.Y));
                maxHeight = MathF.Max(maxHeight, heightMap.GetUpperHeight(pBottom.X, pBottom.Y));
            }

            // set z coordinate to the maximum z that was encountered along the cross section
            const float heightBias = 0.1f;
            maxHeight += heightBias;
            pTop.Z = maxHeight;
            pBottom.Z = maxHeight;
            p.Position = p.Position.WithZ(maxHeight);

            // remember the outermost positions (that's where we'll insert mesh vertices)
            p.TopPosition = pTop;
            p.BottomPosition = pBottom;
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

        protected override (Vector2 top, Vector2 bottom) GetTopBottomTextureCoordinates(float relativeProgress, float distanceAlongRoad)
        {
            var (v, vOffset) = GetVFromAxisAlignedBoundingBox();

            //for crossing: texture should stretch to the base mesh -> interpolate the boundary values
            var u = MathUtility.Lerp(TextureBounds.BottomLeft.X, TextureBounds.BottomRight.X, relativeProgress);
            var uvTop = new Vector2(u, v + vOffset);
            var uvBottom = new Vector2(u, v - vOffset);
            return (uvTop, uvBottom);
        }

    }

    internal abstract class SimpleRoadSegmentMesher : RoadSegmentMesher
    {
        public SimpleRoadSegmentMesher(IRoadSegment segment, float halfHeight, RoadTemplate template)
            : base(segment, halfHeight, template)
        {
        }

        protected Vector3 StartToTopCorner { get; set; }
        protected Vector3 EndToTopCorner { get; set; }

        protected override void Prepare()
        {
            var straightRoad = Segment as StraightRoadSegment;
            if (straightRoad == null)
            {
                throw new InvalidOperationException();
            }

            StartToTopCorner = ToCorner(straightRoad.Start, false);
            EndToTopCorner = ToCorner(straightRoad.End, true);
        }

        protected override Vector3 ToTopBorder(float relativeProgress)
        {
            return Vector3.Lerp(StartToTopCorner, EndToTopCorner, relativeProgress);
        }


        /// <summary>
        /// Generate vector from the start/end position of an edge to the corner of the mesh's base geometry
        /// </summary>
        /// <param name="neighbor"></param>
        /// <param name="atEnd"></param>
        /// <returns></returns>
        protected abstract Vector3 ToCorner(RoadSegmentEndPoint neighbor, bool atEnd);
    }

    internal sealed class StraightRoadSegmentMesher : SimpleRoadSegmentMesher
    {
        public StraightRoadSegmentMesher(IRoadSegment segment, float halfHeight, RoadTemplate template)
            : base(segment, halfHeight, template)
        {
        }

        private float TextureRoadLength { get; set; }
        private float StartTopUOffset { get; set; }
        private float EndTopUOffset { get; set; }

        protected override void Prepare()
        {
            base.Prepare();
            TextureRoadLength = Template.RoadWidth * 4;
            StartTopUOffset = Vector3.Dot(StartToTopCorner, DirectionNoZ) / TextureRoadLength;
            EndTopUOffset = Vector3.Dot(EndToTopCorner, DirectionNoZ) / TextureRoadLength;
        }

        protected override Vector3 ToCorner(RoadSegmentEndPoint neighbor, bool atEnd)
        {
            var neighborNormal = GetNeighborNormal(neighbor, atEnd);
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

        protected override (Vector2 top, Vector2 bottom) GetTopBottomTextureCoordinates(float relativeProgress, float distanceAlongRoad)
        {
            var (v, vOffset) = GetVFromAxisAlignedBoundingBox();

            // for roads: the road segment we are meshing may be much longer or shorter than the texture road length,
            // so stretching the texture on each segment would look odd -> repeat it instead
            var u = distanceAlongRoad / TextureRoadLength;

            var topUOffset = MathUtility.Lerp(StartTopUOffset, EndTopUOffset, relativeProgress);

            var uvTop = new Vector2(u + topUOffset, v + vOffset);
            var uvBottom = new Vector2(u - topUOffset, v - vOffset);
            return (uvTop, uvBottom);
        }

    }


    internal sealed class CurvedRoadSegmentMesher : SimpleRoadSegmentMesher
    {
        public CurvedRoadSegmentMesher(IRoadSegment segment, float halfHeight, RoadTemplate template)
            : base(segment, halfHeight, template)
        {
        }
        
        protected override void Prepare()
        {
            base.Prepare();
        }

        protected override Vector3 ToCorner(RoadSegmentEndPoint neighbor, bool atEnd)
        {
            return HalfHeight * GetNeighborNormal(neighbor, atEnd);
        }

        protected override (Vector2 top, Vector2 bottom) GetTopBottomTextureCoordinates(float relativeProgress, float distanceAlongRoad)
        {
            var top = Vector2.Lerp(TextureBounds.TopLeft, TextureBounds.TopRight, relativeProgress);
            var bottom = Vector2.Lerp(TextureBounds.BottomLeft, TextureBounds.BottomRight, relativeProgress);
            return (top, bottom);
        }
    }
}
