using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Terrain.Roads;

namespace OpenSage.Terrain
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

        public List<RoadNetwork> BuildNetworks()
        {
            var networks = new List<RoadNetwork>();

            // Create one network for each connected set of segments of a specific type.
            var edgesToProcess = new List<RoadTopologyEdge>(Edges);
            while (edgesToProcess.Count > 0)
            {
                var edge = edgesToProcess[edgesToProcess.Count - 1];
                edgesToProcess.RemoveAt(edgesToProcess.Count - 1);

                var seenEdges = new List<RoadTopologyEdge>();
                seenEdges.Add(edge);

                var network = new RoadNetwork(edge.Template);
                networks.Add(network);
                
                void FollowPath(RoadTopologyNode node)
                {
                    foreach (var nextEdge in node.Edges)
                    {
                        if (nextEdge.Template != edge.Template
                            || seenEdges.Contains(nextEdge))
                        {
                            continue;
                        }

                        network.AddSegment(new AngledRoadSegment(nextEdge.Start.Position, nextEdge.End.Position));
                        edgesToProcess.Remove(nextEdge);

                        seenEdges.Add(nextEdge);

                        FollowPath(nextEdge.Start);
                        FollowPath(nextEdge.End);
                    }
                }

                FollowPath(edge.Start);
                FollowPath(edge.End);
            }

            return networks;
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
