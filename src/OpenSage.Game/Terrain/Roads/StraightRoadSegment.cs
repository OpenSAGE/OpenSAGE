using System.Collections.Generic;
using System.Numerics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class StraightRoadSegment : ISimpleRoadSegment
    {
        public RoadSegmentEndPoint Start { get; }
        public RoadSegmentEndPoint End { get; }
        public Vector3 StartPosition => Start.Position;
        public Vector3 EndPosition => End.Position;
        public bool MirrorTexture => false;
        public RoadTextureType Type => RoadTextureType.Straight;

        public StraightRoadSegment(Vector3 start, Vector3 end)
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

        public RoadSegmentMesher CreateMesher(RoadTemplate template)
        {
            var halfWidth = template.RoadWidth * template.RoadWidthInTexture / 2f;
            return new StraightRoadSegmentMesher(this, halfWidth, template);
        }
    }
}
