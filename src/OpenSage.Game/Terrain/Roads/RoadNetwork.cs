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

        private static IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> BuildEdgeSegments(RoadTopology topology)
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
                            connectedEdgeSegment.Start.ConnectTo(edgeSegment, Vector3.Normalize(direction));
                        }
                        else
                        {
                            connectedEdgeSegment.End.ConnectTo(edgeSegment, Vector3.Normalize(direction));
                        }
                    }
                }
            }

            return edgeSegments;
        }

        private static void InsertNodeSegments(RoadTopology topology, IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            foreach (var node in topology.Nodes)
            {
                foreach (var edgesPerTemplate in node.Edges.GroupBy(e => e.Template))
                {
                    var template = edgesPerTemplate.Key;
                    // possible optimization: only compute angles if necessary?
                    var incomingRoadData = ComputeRoadAngles(node, edgesPerTemplate);

                    switch (edgesPerTemplate.Count())
                    {
                        // TODO support end caps
                        case 1: // end point
                            break;
                        case 2:
                            CurvedRoadSegment.CreateCurve(incomingRoadData, node.Position, template, edgeSegments);
                            break;
                        case 3:
                        case 4:
                            CrossingRoadSegment.CreateCrossing(incomingRoadData, node.Position, template, edgeSegments);
                            break;
                    }
                }
            }
        }

        private static IReadOnlyList<IncomingRoadData> ComputeRoadAngles(RoadTopologyNode node, IEnumerable<RoadTopologyEdge> edges)
        {
            if (edges.Count() < 2)
            {
                return Array.Empty<IncomingRoadData>();
            }

            IncomingRoadData GetIncomingRoadData(RoadTopologyNode node, RoadTopologyEdge incomingEdge)
            {
                var isStart = incomingEdge.Start.Position == node.Position;
                var targetNodePosition = isStart ? incomingEdge.End.Position : incomingEdge.Start.Position;
                var roadVector = targetNodePosition - node.Position;
                var direction = roadVector.LengthSquared() < 0.01f ? Vector3.UnitX : Vector3.Normalize(roadVector);

                return new IncomingRoadData(
                    incomingEdge,
                    targetNodePosition,
                    direction,
                    MathF.Atan2(direction.Y, direction.X));
            }

            var incomingRoads = edges
                .Select(e => GetIncomingRoadData(node, e))
                .OrderBy(d => d.AngleToAxis)
                .ToList();

            for (var i = 1; i < incomingRoads.Count; ++i)
            {
                incomingRoads[i].Previous = incomingRoads[i - 1];
                incomingRoads[i].AngleToPreviousEdge = incomingRoads[i].AngleToAxis - incomingRoads[i - 1].AngleToAxis;
            }

            incomingRoads[0].Previous = incomingRoads[incomingRoads.Count - 1];
            incomingRoads[0].AngleToPreviousEdge = 2 * MathF.PI + incomingRoads[0].AngleToAxis - incomingRoads[incomingRoads.Count - 1].AngleToAxis;

            return incomingRoads;
        }

        private static IList<RoadNetwork> BuildNetworks(RoadTopology topology, IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
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
