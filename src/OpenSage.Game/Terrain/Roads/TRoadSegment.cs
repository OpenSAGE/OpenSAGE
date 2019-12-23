using System.Collections.Generic;
using OpenSage.Graphics.Shaders;

namespace OpenSage.Terrain.Roads
{
    internal sealed class TRoadSegment : IRoadSegment
    {
        public TRoadSegment(RoadSegmentEndPoint top, RoadSegmentEndPoint right, RoadSegmentEndPoint bottom)
        {
            Top = top;
            Right = right;
            Bottom = bottom;
        }

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

        public void GenerateMesh(RoadTemplate roadTemplate, HeightMap heightMap, List<RoadShaderResources.RoadVertex> vertices, List<ushort> indices)
        {

        }
    }
}
