using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenSage.Terrain.Roads
{
    internal sealed class RoadNetwork
    {
        private readonly List<IRoadSegment> _segments;

        public RoadTemplate Template { get; }
        public IReadOnlyList<IRoadSegment> Segments => _segments;

        public RoadNetwork(RoadTemplate template)
        {
            Template = template;
            _segments = new List<IRoadSegment>();
        }

        public static IList<RoadNetwork> BuildNetworks(RoadTopology topology)
        {
            var edgeSegments = BuildEdgeSegments(topology);
            InsertNodeSegments(topology, edgeSegments);
            var networks = BuildNetworks(topology, edgeSegments);

            return networks;
        }

        private static IDictionary<RoadTopologyEdge, StraightRoadSegment> BuildEdgeSegments(RoadTopology topology)
        {
            // create a dictionary from edges to segments
            var edgeSegments = topology.Edges.ToDictionary(e => e, e => new StraightRoadSegment(e.Start.Position, e.End.Position));

            // create end points and connect them to the neighbour edges
            foreach (var edge in topology.Edges)
            {
                var edgeSegment = edgeSegments[edge];

                Connect(edge.Start, edge.Start.Position - edge.End.Position);
                Connect(edge.End, edge.End.Position - edge.Start.Position);

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
            }

            return edgeSegments;
        }

        private static IEnumerable<IncomingRoadData> ComputeRoadAngles(RoadTopologyNode node, IEnumerable<RoadTopologyEdge> edges)
        {
            var incomingRoads = edges.Select(topologyEdge =>
            {
                var incoming = new IncomingRoadData();
                incoming.TopologyEdge = topologyEdge;
                incoming.TargetNodePosition = (topologyEdge.Start.Position == node.Position) ? topologyEdge.End.Position : topologyEdge.Start.Position;
                incoming.Direction = Vector3.Normalize(incoming.TargetNodePosition - node.Position);
                incoming.AngleToAxis = Math.Atan2(incoming.Direction.Y, incoming.Direction.X);
                return incoming;
            }).ToList();

            incomingRoads.Sort((first, second) => first.AngleToAxis.CompareTo(second.AngleToAxis));
            var n = incomingRoads.Count;

            for (var i = 1; i < n; ++i)
            {
                incomingRoads[i].Previous = incomingRoads[i - 1];
                incomingRoads[i].AngleToPreviousEdge = incomingRoads[i].AngleToAxis - incomingRoads[i - 1].AngleToAxis;
            }
            incomingRoads[0].Previous = incomingRoads[n - 1];
            incomingRoads[0].AngleToPreviousEdge = 2 * Math.PI + incomingRoads[0].AngleToAxis - incomingRoads[n - 1].AngleToAxis;
            
            return incomingRoads;
        }

        private static CrossingType ChooseCrossingType(IEnumerable<IncomingRoadData> incomingRoads)
        {
            var angles = incomingRoads.Select(road => road.AngleToPreviousEdge).ToList();
            angles.Sort();

            if (angles.Count() == 3)
            {
                if (angles[2] < Math.PI * 0.9)
                {
                    return CrossingType.SymmetricY;
                }
                if (angles[1] - angles[0] < Math.PI * 0.25)
                {
                    return CrossingType.T;
                }
                return CrossingType.AsymmetricY;
            }
            return CrossingType.X;
        }

        private static void InsertNodeSegments(RoadTopology topology, IDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            foreach (var node in topology.Nodes)
            {
                foreach (var edgesPerTemplate in node.Edges.GroupBy(e => e.Template))
                {
                    switch (edgesPerTemplate.Count())
                    {
                        // TODO support end caps
                        case 1: // end point
                            break;
                        case 2: // TODO normal road, create segments for tight/broad curves
                            break;
                        case 3:
                            var template = edgesPerTemplate.Key;
                            var edgedata = ComputeRoadAngles(node, edgesPerTemplate);
                            var crossingType = ChooseCrossingType(edgedata);
                            switch(crossingType)
                            {
                                case CrossingType.T:
                                    CrossingRoadSegment.CreateTCrossing(edgedata, node.Position, template, edgeSegments);
                                    break;
                                case CrossingType.AsymmetricY:
                                    CrossingRoadSegment.CreateYAsymmCrossing(edgedata, node.Position, template, edgeSegments);
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        private static IList<RoadNetwork> BuildNetworks(RoadTopology topology, IDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            var networks = new List<RoadNetwork>();

            // Create one network for each connected set of segments of a specific type.
            foreach (var templateEdges in topology.Edges.GroupBy(e => e.Template))
            {
                var edgesToProcess = new HashSet<IRoadSegment>(templateEdges.Select(e => edgeSegments[e]));

                while (edgesToProcess.Any())
                {
                    var edgeSegment = edgesToProcess.First();
                    edgesToProcess.Remove(edgeSegment);

                    var seenSegments = new HashSet<IRoadSegment>();
                    seenSegments.Add(edgeSegment);

                    var network = new RoadNetwork(templateEdges.Key);
                    networks.Add(network);

                    network._segments.Add(edgeSegment);

                    foreach (var endPoint in edgeSegment.EndPoints)
                    {
                        FollowPath(endPoint);
                    }

                    void FollowPath(RoadSegmentEndPoint endPoint)
                    {
                        if (endPoint.To == null || seenSegments.Contains(endPoint.To))
                            return;

                        edgesToProcess.Remove(endPoint.To);
                        network._segments.Add(endPoint.To);
                        seenSegments.Add(endPoint.To);

                        foreach (var nextEndPoint in endPoint.To.EndPoints)
                        {
                            FollowPath(nextEndPoint);
                        }
                    }
                }
            }

            return networks;
        }
    }
}
