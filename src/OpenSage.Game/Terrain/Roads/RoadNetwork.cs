using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;

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

        public static IEnumerable<RoadNetwork> BuildNetworks(RoadTopology topology, RoadTemplateList roadTemplateList)
        {
            topology.AlignOrientation();
            var edgeSegments = BuildEdgeSegments(topology);
            InsertCrossingSegments(topology, edgeSegments);
            InsertCurveSegments(topology, edgeSegments);
            InsertEndCapSegments(edgeSegments, roadTemplateList);
            var networks = BuildNetworks(topology, edgeSegments);

            // sort networks in the order specified by roadTemplateList
            var sortedNetworks = roadTemplateList
                .Join(
                    networks,
                    t => t.InstanceId,
                    n => n.Template.InstanceId,
                    (t, n) => n);                

            return sortedNetworks;
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

        private static void InsertCrossingSegments(RoadTopology topology, IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            foreach (var node in topology.Nodes)
            {
                foreach (var edgesPerTemplate in node.Edges.GroupBy(e => e.Template))
                {
                    var connectedEdges = edgesPerTemplate.Count();
                    if (connectedEdges == 3 || connectedEdges == 4)
                    {
                        var incomingRoadData = ComputeRoadAngles(node, edgesPerTemplate, edgeSegments);
                        CrossingRoadSegment.CreateCrossing(incomingRoadData, node.Position, edgesPerTemplate.Key, edgeSegments);
                    }
                }
            }
        }

        private static void InsertCurveSegments(RoadTopology topology, IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            foreach (var node in topology.Nodes)
            {
                foreach (var edgesPerTemplate in node.Edges.GroupBy(e => e.Template))
                {
                    var connectedEdges = edgesPerTemplate.Count();
                    if (connectedEdges == 2)
                    {
                        var incomingRoadData = ComputeRoadAngles(node, edgesPerTemplate, edgeSegments);
                        CurvedRoadSegment.CreateCurve(incomingRoadData, node.Position, edgesPerTemplate.Key, edgeSegments);
                    }
                }
            }
        }

        private static void InsertEndCapSegments(IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments, RoadTemplateList roadTemplateList)
        {
            foreach (var edge in edgeSegments.Reverse())
            {
                // the end cap flag is only relevant when the edge is not connected to another edge on this end
                bool hasEndCapAtStart = edge.Key.StartType.HasFlag(RoadType.EndCap) && edge.Value.Start.To == null;
                bool hasEndCapAtEnd = edge.Key.EndType.HasFlag(RoadType.EndCap) && edge.Value.End.To == null;

                // single edges without any connected edges can only have one end cap (at the end position), even when the flag is present at both nodes
                if (hasEndCapAtEnd)
                {
                    EndCapRoadSegment.CreateEndCap(
                        GetIncomingRoadData(edge.Key.End, edge.Key, edgeSegments[edge.Key]),
                        edge.Value.EndPosition,
                        edge.Key.Template,
                        edgeSegments,
                        roadTemplateList);
                }
                else if (hasEndCapAtStart)
                {
                    EndCapRoadSegment.CreateEndCap(
                        GetIncomingRoadData(edge.Key.Start, edge.Key, edgeSegments[edge.Key]),
                        edge.Value.StartPosition,
                        edge.Key.Template,
                        edgeSegments,
                        roadTemplateList);
                }
            }
        }

        private static IReadOnlyList<IncomingRoadData> ComputeRoadAngles(RoadTopologyNode node, IEnumerable<RoadTopologyEdge> edges, IReadOnlyDictionary<RoadTopologyEdge, StraightRoadSegment> edgeSegments)
        {
            if (edges.Count() < 2)
            {
                return Array.Empty<IncomingRoadData>();
            }

            var incomingRoads = edges
                .Select(e => GetIncomingRoadData(node, e, edgeSegments[e]))
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

        private static IncomingRoadData GetIncomingRoadData(RoadTopologyNode node, RoadTopologyEdge incomingEdge, StraightRoadSegment edgeSegment)
        {
            var isStart = incomingEdge.Start.Position == node.Position;
            var fromPosition = isStart ? edgeSegment.EndPosition : edgeSegment.StartPosition;
            var segmentVector = (edgeSegment.EndPosition - edgeSegment.StartPosition) * (isStart ? 1 : -1);
            var direction = segmentVector.LengthSquared() < 0.01f ? Vector3.UnitX : Vector3.Normalize(segmentVector);

            return new IncomingRoadData(
                incomingEdge,
                fromPosition,
                direction,
                MathF.Atan2(direction.Y, direction.X));
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
