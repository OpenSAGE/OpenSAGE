using System;
using System.Collections.Generic;
using System.Linq;
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
            // ignore duplicate segments (e.g. in Alpine Assault)
            if (Edges.Any(e => (e.Start.Position == start.Position && e.End.Position == end.Position) ||
                               (e.End.Position == start.Position && e.Start.Position == end.Position)))
            {
                return;
            }

            var startNode = GetOrCreateNode(start.Position);
            var endNode = GetOrCreateNode(end.Position);

            if (endNode == startNode)
            {
                // create a new dummy node, otherwise this edge gets counted twice as incoming edge of startNode
                // add a small offset to make sure that other map objects use the 'normal' node
                endNode = GetOrCreateNode(end.Position + 0.001f * Vector3.UnitX);
            }

            var edge = new RoadTopologyEdge(
                template,
                startNode,
                start.RoadType,
                endNode,
                end.RoadType,
                Edges.Count);

            Edges.Add(edge);

            startNode.Edges.Add(edge);
            endNode.Edges.Add(edge);
        }

        /// <summary>
        /// Aligns the orientation of connected road segments so that textures with
        /// text on them are rotated the same way.
        /// </summary>
        public void AlignOrientation()
        {
            // Basically, edges with higher indices are rotated to match edges with lower indices.
            // This is a bit counter-intuitive, as it is opposed to the creation order (the edge
            // that was created first is the last in the list and vice versa.

            // At crossings, the second edge is rotated to match the first edge. All other edges
            // are not affected.
            foreach (var edge in Edges)
            {
                // If we already aligned this edge, skip to the next.
                if (edge.AlignedLikeIndex >= 0)
                    continue;
                
                edge.AlignedLikeIndex = edge.Index;

                // Walk along connected edges in both directions,
                // aligning them to the current edge.
                WalkEdges(edge, edge.Start);
                WalkEdges(edge, edge.End);

                void WalkEdges(RoadTopologyEdge currentEdge, RoadTopologyNode currentNode)
                {
                    // Get the next edge connected to the current one via currentNode.
                    var nextEdge = GetNextEdge(currentEdge, currentNode);

                    while (nextEdge != null)
                    {
                        // Align the next edge by swapping the end points if necessary.
                        nextEdge.AlignedLikeIndex = currentEdge.AlignedLikeIndex;
                        if (nextEdge.Start.Position == currentEdge.Start.Position ||
                            nextEdge.End.Position == currentEdge.End.Position)
                        {
                            nextEdge.SwapEndpoints();
                        }

                        // Continue along the path.
                        currentEdge = nextEdge;
                        currentNode = currentNode == currentEdge.Start ? currentEdge.End : currentEdge.Start;
                        nextEdge = GetNextEdge(currentEdge, currentNode);
                    }
                }
            }

            RoadTopologyEdge GetNextEdge(RoadTopologyEdge edge, RoadTopologyNode node)
            {
                // If there is only one edge, we have reached the end of the path.
                if (node.Edges.Count > 1)
                {
                    // If edge is the first edge, ...
                    if (edge == node.Edges[0])
                    {
                        // ... and the second edge is not yet aligned, we can move
                        // along the second edge...
                        if (node.Edges[1].AlignedLikeIndex < 0)
                        {
                            return node.Edges[1];
                        }
                    }
                    else if (edge == node.Edges[1])
                    {
                        // ...and vice versa.
                        if (node.Edges[0].AlignedLikeIndex < 0)
                        {
                            return node.Edges[0];
                        }
                    }
                }

                // No more edges along this path.
                return null;
            }
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

        public RoadTopologyNode Start { get; private set; }
        public RoadType StartType { get; private set; }

        public RoadTopologyNode End { get; private set; }
        public RoadType EndType { get; private set; }

        public int Index { get; }
        public int AlignedLikeIndex { get; set; } = -1;

        public RoadTopologyEdge(
            RoadTemplate template,
            RoadTopologyNode start,
            RoadType startType,
            RoadTopologyNode end,
            RoadType endType,
            int index)
        {
            Template = template;

            Start = start;
            StartType = startType;

            End = end;
            EndType = endType;

            Index = index;
        }

        public void SwapEndpoints()
        {
            var temp = Start;
            Start = End;
            End = temp;

            // This is actually a bug in the original engine:
            // We should swap StartType and EndType as well,
            // but then we couldn't recreate the original behavior.

            // var tempType = StartType;
            // StartType = EndType;
            // EndType = tempType;
        }
    }
}
