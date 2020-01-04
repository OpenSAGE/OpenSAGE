using System.Collections.Generic;
using System.Numerics;

namespace OpenSage.Terrain.Roads
{
    internal interface IRoadSegment
    {
        public Vector3 StartPosition { get; }
        public Vector3 EndPosition { get; }

        public RoadTextureType Type { get; }
        public bool MirrorTexture { get; }

        IEnumerable<RoadSegmentEndPoint> EndPoints { get; }

        RoadSegmentMesher CreateMesher(RoadTemplate template);
    }
}
