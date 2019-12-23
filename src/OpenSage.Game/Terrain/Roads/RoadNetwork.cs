using System.Collections.Generic;

namespace OpenSage.Terrain.Roads
{
    internal sealed class RoadNetwork
    {
        public RoadTemplate Template { get; }
        public List<IRoadSegment> Segments { get; }

        public RoadNetwork(RoadTemplate template)
        {
            Template = template;
            Segments = new List<IRoadSegment>();
        }

        internal void AddSegment(IRoadSegment segment)
        {
            this.Segments.Add(segment);
        }
    }
}
