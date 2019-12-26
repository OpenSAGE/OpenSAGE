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

        private void AddSegment(IRoadSegment segment)
        {
            this._segments.Add(segment);
        }

        public static IList<RoadNetwork> BuildNetworks(RoadTopology topology)
        {
            var edgeSegments = BuildEdgeSegments(topology);
            InsertNodeSegments(topology, edgeSegments);
            var networks = BuildNetworks(topology, edgeSegments);

            return networks;
        }

        private static IDictionary<RoadTopologyEdge, AngledRoadSegment> BuildEdgeSegments(RoadTopology topology)
        {
            // create a dictionary from edges to segments
            var edgeSegments = topology.Edges.ToDictionary(e => e, e => new AngledRoadSegment(e.Start.Position, e.End.Position));

            // create end points and connect them to the neighbour edges
            foreach (var edge in topology.Edges)
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

            return edgeSegments;
        }

        private static void InsertNodeSegments(RoadTopology topology, IDictionary<RoadTopologyEdge, AngledRoadSegment> edgeSegments)
        {
            foreach (var node in topology.Nodes)
            {
                foreach (var edgesPerTemplate in node.Edges.GroupBy(e => e.Template))
                {
                    switch (edgesPerTemplate.Count())
                    {
                        case 1: // end point
                            break;
                        case 2: // TODO normal road, create segments for tight/broad curves
                            break;
                        case 3:
                            var halfWidth = edgesPerTemplate.Key.RoadWidth / 2;

                            // TODO figure out orientation and endpoints
                            var segment = new TRoadSegment(
                                node.Position,
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

                            // TODO consider ordering of edges

                            Connect(edgesPerTemplate.ElementAt(0), segment.Top, Vector3.UnitY);
                            Connect(edgesPerTemplate.ElementAt(1), segment.Right, Vector3.UnitX);
                            Connect(edgesPerTemplate.ElementAt(2), segment.Bottom, -Vector3.UnitY);

                            break;
                    }
                }
            }
        }

        private static IList<RoadNetwork> BuildNetworks(RoadTopology topology, IDictionary<RoadTopologyEdge, AngledRoadSegment> edgeSegments)
        {
            var networks = new List<RoadNetwork>();

            // Create one network for each connected set of segments of a specific type.
            foreach (var templateEdges in topology.Edges.GroupBy(e => e.Template))
            {
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
}
