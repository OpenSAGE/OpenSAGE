using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Shaders;

namespace OpenSage.Terrain.Roads
{
    internal interface IRoadSegment
    {
        public Vector3 StartPosition { get; }
        public Vector3 EndPosition { get; }

        public RoadTextureType Type { get; }
        public bool MirrorTexture { get; }

        IEnumerable<RoadSegmentEndPoint> EndPoints { get; }

        void GenerateMesh(
            RoadTemplate template,
            HeightMap heightMap,
            List<RoadShaderResources.RoadVertex> vertices,
            List<ushort> indices);
    }
}
