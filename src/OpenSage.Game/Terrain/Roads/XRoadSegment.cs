using System.Collections.Generic;
using OpenSage.Graphics.Shaders;

namespace OpenSage.Terrain.Roads
{
    internal sealed class XRoadSegment : IRoadSegment
    {
        public XRoadSegment(
            RoadSegmentEndPoint left,
            RoadSegmentEndPoint top,
            RoadSegmentEndPoint right,
            RoadSegmentEndPoint bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public RoadSegmentEndPoint Left { get; }
        public RoadSegmentEndPoint Top { get; }
        public RoadSegmentEndPoint Right { get; }
        public RoadSegmentEndPoint Bottom { get; }

        public IEnumerable<RoadSegmentEndPoint> EndPoints
        {
            get
            {
                yield return Left;
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
