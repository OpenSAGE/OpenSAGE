using System;
using System.Collections.Generic;
using Priority_Queue;

namespace OpenSage.Navigation
{
    [Flags]
    public enum Passability
    {
        Impassable,
        Passable,
        ImpassableForTeams,
        ImpassableForAir,
    }

    public class Node : FastPriorityQueueNode
    {
        private const int CardinalDirCost = 10;
        // A very accurate appoximation for sqrt(2) * 10
        private const int DiagonalDirCost = 14;

        private readonly Graph _graph;
        public Passability Passability { get; set; }
        public int X { get; }
        public int Y { get; }

        internal Node(Graph graph, int x, int y)
        {
            X = x;
            Y = y;
            _graph = graph;
            Passability = Passability.Passable;
        }

        internal bool IsPassable => Passability == Passability.Passable;

        // Diagonal distance heuristic (http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html)
        // Integer variant inspired by this comment: https://stackoverflow.com/a/32635858
        internal int EstimateDistance(Node target)
        {
            var dx = Math.Abs(X - target.X);
            var dy = Math.Abs(Y - target.Y);
            return CardinalDirCost * (dx + dy) + (DiagonalDirCost - 2 * CardinalDirCost) * Math.Min(dx, dy);
        }

        internal void GetAdjacentNodes(List<(Node, int)> adjacentNodes, Passability requiredPassability = Passability.Passable)
        {
            foreach (var ((dx, dy), cost) in _neighbours)
            {
                if (_graph.TryGetNode(X + dx, Y + dy, out var node) && node.Passability.HasFlag(requiredPassability))
                {
                    adjacentNodes.Add((node, cost));
                }
            }
        }

        private readonly ((int, int), int)[] _neighbours = {
            // Top
            ((0, -1), CardinalDirCost),
            // Top left
            ((-1, -1), DiagonalDirCost),
            // Left
            ((-1, 0), CardinalDirCost),
            // Bottom left
            ((-1, 1), DiagonalDirCost),
            // Bottom
            ((0, 1), CardinalDirCost),
            // Bottom right
            ((1, 1), DiagonalDirCost),
            // Right
            ((1, 0), CardinalDirCost),
            // Top right
            ((1, -1), DiagonalDirCost)
        };
    }
}
