using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Ini;
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

            var edgeSegments = Edges.ToDictionary(e => e, e => new AngledRoadSegment(e.Start.Position, e.End.Position));
            var nodeSegments = new Dictionary<RoadTopologyNode, IRoadSegment>();

            foreach (var edge in Edges)
            {
                var edgeSegment = edgeSegments[edge];

                void Connect(RoadTopologyNode node, in Vector3 direction)
                {
                    foreach (var connectedEdge in node.Edges)
                    {
                        if (connectedEdge == edge || connectedEdge.Template != edge.Template)
                        {
                            continue;
                        }

                        var connectedEdgeSegment = edgeSegments[connectedEdge];

                        if (connectedEdge.Start.Position == node.Position)
                        {
                            connectedEdgeSegment.Start.ConnectTo(edgeSegment, direction);
                        }
                        else
                        {
                            connectedEdgeSegment.End.ConnectTo(edgeSegment, direction);
                        }
                    }
                }

                Connect(edge.Start, edge.Start.Position - edge.End.Position);
                Connect(edge.End, edge.End.Position - edge.Start.Position);
            }

            foreach (var node in Nodes)
            {
                foreach (var edgesPerTemplate in node.Edges.GroupBy(e => e.Template))
                {
                    switch (edgesPerTemplate.Count())
                    {
                        case 1: // end point
                            break;
                        case 2: // normal road, create segments for tight/broad curves
                            break;
                        case 3:
                            var halfWidth = edgesPerTemplate.Key.RoadWidth / 2;

                            var segment = new TRoadSegment(
                                new RoadSegmentEndPoint(node.Position + new Vector3(0, halfWidth, 0)),
                                new RoadSegmentEndPoint(node.Position + new Vector3(halfWidth, 0, 0)),
                                new RoadSegmentEndPoint(node.Position + new Vector3(0, -halfWidth, 0)));

                            void Connect(RoadTopologyEdge edge, RoadSegmentEndPoint endPoint, in Vector3 direction)
                            {
                                var edgeSegment = edgeSegments[edge];
                                if (edge.Start.Position == node.Position)
                                {
                                    edgeSegment.Start.Position = endPoint.Position;
                                    edgeSegment.Start.ConnectTo(segment, direction);
                                    endPoint.ConnectTo(edgeSegment, edge.Start.Position - edge.End.Position);
                                }
                                else
                                {
                                    edgeSegment.End.Position = endPoint.Position;
                                    edgeSegment.End.ConnectTo(segment, direction);
                                    endPoint.ConnectTo(edgeSegment, edge.End.Position - edge.Start.Position);
                                }
                            }

                            Connect(edgesPerTemplate.ElementAt(0), segment.Top, Vector3.UnitY);
                            Connect(edgesPerTemplate.ElementAt(1), segment.Right, Vector3.UnitX);
                            Connect(edgesPerTemplate.ElementAt(2), segment.Bottom, -Vector3.UnitY);

                            break;
                    }
                }
            }

            foreach (var templateEdges in Edges.GroupBy(e => e.Template))
            {
                // Create one network for each connected set of segments of a specific type.
                var edgesToProcess = new List<AngledRoadSegment>(templateEdges.Select(e => edgeSegments[e]));

                while (edgesToProcess.Count > 0)
                {
                    var edgeSegment = edgesToProcess[edgesToProcess.Count - 1];
                    edgesToProcess.RemoveAt(edgesToProcess.Count - 1);

                    var seenSegments = new List<IRoadSegment>();
                    seenSegments.Add(edgeSegment);

                    var network = new RoadNetwork(templateEdges.Key);
                    networks.Add(network);

                    network.AddSegment(edgeSegment);

                    void FollowPath(RoadSegmentEndPoint endPoint)
                    {
                        if (endPoint.To == null || seenSegments.Contains(endPoint.To))
                            return;

                        network.AddSegment(endPoint.To);
                        seenSegments.Add(endPoint.To);

                        foreach (var nextEndPoint in endPoint.To.EndPoints)
                        {
                            FollowPath(nextEndPoint);
                        }
                    }

                    foreach (var endPoint in edgeSegment.EndPoints)
                    {
                        FollowPath(endPoint);
                    }
                }
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

    //internal enum RoadNodeType
    //{
    //    Endpoint,
    //    TwoWay,
    //    ThreeWay,
    //    FourWay
    //}

    //internal sealed class RoadNetwork
    //{
    //    public RoadTemplate Template { get; }

    //    public List<RoadNetworkNode> Nodes { get; } = new List<RoadNetworkNode>();
    //    public List<RoadNetworkEdge> Edges { get; } = new List<RoadNetworkEdge>();

    //    public RoadNetwork(RoadTemplate template)
    //    {
    //        Template = template;
    //    }

    //    public void AddEdge(RoadTopologyEdge topologyEdge)
    //    {
    //        var startNode = GetOrCreateNode(topologyEdge.Start);
    //        var endNode = GetOrCreateNode(topologyEdge.End);

    //        var edge = new RoadNetworkEdge(
    //            topologyEdge,
    //            startNode,
    //            endNode);

    //        Edges.Add(edge);

    //        startNode.Edges.Add(edge);
    //        endNode.Edges.Add(edge);
    //    }

    //    private RoadNetworkNode GetOrCreateNode(RoadTopologyNode topologyNode)
    //    {
    //        var node = Nodes.Find(x => x.TopologyNode == topologyNode);
    //        if (node == null)
    //        {
    //            Nodes.Add(node = new RoadNetworkNode(topologyNode));
    //        }
    //        return node;
    //    }
    //}

    //internal sealed class RoadNetworkNode
    //{
    //    public RoadTopologyNode TopologyNode { get; }
    //    public List<RoadNetworkEdge> Edges { get; } = new List<RoadNetworkEdge>();

    //    public RoadNodeType NodeType { get; private set; }

    //    public RoadNetworkNode(RoadTopologyNode topologyNode)
    //    {
    //        TopologyNode = topologyNode;
    //    }

    //    public void ClassifyType()
    //    {
    //        // TODO
    //    }
    //}

    //internal sealed class RoadNetworkEdge
    //{
    //    public RoadTopologyEdge TopologyEdge { get; }

    //    public RoadNetworkNode Start { get; }
    //    public RoadNetworkNode End { get; }

    //    public RoadNetworkEdge(
    //        RoadTopologyEdge topologyEdge,
    //        RoadNetworkNode start,
    //        RoadNetworkNode end)
    //    {
    //        TopologyEdge = topologyEdge;
    //        Start = start;
    //        End = end;
    //    }
    //}
}
