using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Map;

namespace OpenSage.Terrain.Roads
{
    internal sealed class RoadTopology
    {
        public List<RoadTopologyNode> Nodes { get; } = new List<RoadTopologyNode>();
        public List<RoadTopologyEdge> Edges { get; } = new List<RoadTopologyEdge>();

        public void AddSegment(RoadTemplate template, MapObject start, MapObject end)
        {
            var startNode = GetOrCreateNode(start);
            var endNode = GetOrCreateNode(end);

            var edge = new RoadTopologyEdge(
                template,
                startNode,
                start.RoadType,
                endNode,
                end.RoadType);

            Edges.Add(edge);

            startNode.Edges.Add(edge);
            endNode.Edges.Add(edge);
        }

        private RoadTopologyNode GetOrCreateNode(MapObject mapObject)
        {
            var node = Nodes.Find(x => x.Position == mapObject.Position);
            if (node == null)
            {
                Nodes.Add(node = new RoadTopologyNode(mapObject.Position));
            }
            return node;
        }
    }

    internal sealed class RoadTopologyNode
    {
        public Vector3 Position { get; }
        public List<RoadTopologyEdge> Edges { get; } = new List<RoadTopologyEdge>();

        public RoadTopologyNode(in Vector3 position)
        {
            Position = position;
        }
    }

    internal sealed class RoadTopologyEdge
    {
        public RoadTemplate Template { get; }

        public RoadTopologyNode Start { get; }
        public RoadType StartType { get; }

        public RoadTopologyNode End { get; }
        public RoadType EndType { get; }

        public RoadTopologyEdge(
            RoadTemplate template,
            RoadTopologyNode start,
            RoadType startType,
            RoadTopologyNode end,
            RoadType endType)
        {
            Template = template;

            Start = start;
            StartType = startType;

            End = end;
            EndType = endType;
        }
    }
}
