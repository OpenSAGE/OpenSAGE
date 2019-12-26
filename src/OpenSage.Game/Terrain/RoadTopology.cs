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
