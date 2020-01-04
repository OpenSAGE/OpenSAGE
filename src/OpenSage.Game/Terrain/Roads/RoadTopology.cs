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
            var startNode = GetOrCreateNode(start.Position);
            var endNode = GetOrCreateNode(end.Position);

            if (endNode == startNode)
            {
                // create a new dummy vertex, otherwise this edge gets counted twice as incoming edge of startVertex
                // add a small offset to make sure that other map objects use the 'normal' vertex
                endNode = GetOrCreateNode(end.Position + 0.0001f * Vector3.UnitX);
            }

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

        private RoadTopologyNode GetOrCreateNode(Vector3 position)
        {
            var node = Nodes.Find(x => x.Position == position);
            if (node == null)
            {
                Nodes.Add(node = new RoadTopologyNode(position));
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
