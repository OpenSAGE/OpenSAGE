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

        //IRoadSegment:
        public Vector3 StartPosition => Start.Position;
        public Vector3 EndPosition => End.Position;
        public bool MirrorTexture => false;
        public RoadTextureType Type => RoadTextureType.Straight;

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
            var halfWidth = template.RoadWidth * template.RoadWidthInTexture / 2f;
            var mesher = new StraightRoadMesher(this, halfWidth, template);
            mesher.GenerateMesh(heightMap, vertices, indices);
        }
    }
}
