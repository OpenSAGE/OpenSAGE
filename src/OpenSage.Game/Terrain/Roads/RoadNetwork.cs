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

            foreach (var network in networks)
            {
                MergeAngledSegments(network);
            }

            return networks;
        }

        private static void MergeAngledSegments(RoadNetwork network)
        {
            var segmentsToProcess = new HashSet<AngledRoadSegment>(network.Segments.OfType<AngledRoadSegment>());

            var intermediatePoints = new LinkedList<Vector3>();

            while (segmentsToProcess.Any())
            {
                intermediatePoints.Clear();

                var segment = segmentsToProcess.First();
                segmentsToProcess.Remove(segment);

                var start = FollowPath(segment.Start, true);
                var end = FollowPath(segment.End, false);

                if (intermediatePoints.Any())
                {
                    network._segments.Remove(segment);
                    network._segments.Add(new AngledRoadSegment(start, end, intermediatePoints.ToList()));
                }
            }

            Vector3 FollowPath(RoadSegmentEndPoint endPoint, bool prepend)
            {
                while (endPoint.To is AngledRoadSegment nextSegment && segmentsToProcess.Contains(nextSegment))
                {
                    if (prepend)
                        intermediatePoints.AddFirst(endPoint.Position);
                    else
                        intermediatePoints.AddLast(endPoint.Position);

                    if (nextSegment.Start.Position == endPoint.Position)
                        endPoint = nextSegment.End;
                    else
                        endPoint = nextSegment.Start;

                    segmentsToProcess.Remove(nextSegment);
                    network._segments.Remove(nextSegment);
                }

                return endPoint.Position;
            }
        }

        private static IDictionary<RoadTopologyEdge, AngledRoadSegment> BuildEdgeSegments(RoadTopology topology)
        {
            // create a dictionary from edges to segments
            var edgeSegments = topology.Edges.ToDictionary(e => e, e => new AngledRoadSegment(e.Start.Position, e.End.Position));

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

        private static void InsertNodeSegments(RoadTopology topology, IDictionary<RoadTopologyEdge, AngledRoadSegment> edgeSegments)
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
                            // TODO figure out orientation and endpoints
                            // TODO support Y segments
                            var halfWidth = edgesPerTemplate.Key.RoadWidth / 2;

                            var segment = new TRoadSegment(
                                node.Position,
                                new RoadSegmentEndPoint(node.Position + new Vector3(0, halfWidth, 0)),
                                new RoadSegmentEndPoint(node.Position + new Vector3(halfWidth, 0, 0)),
                                new RoadSegmentEndPoint(node.Position + new Vector3(0, -halfWidth, 0)));

                            // TODO consider ordering of edges

                            Connect(edgesPerTemplate.ElementAt(0), segment.Top, Vector3.UnitY);
                            Connect(edgesPerTemplate.ElementAt(1), segment.Right, Vector3.UnitX);
                            Connect(edgesPerTemplate.ElementAt(2), segment.Bottom, -Vector3.UnitY);

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
