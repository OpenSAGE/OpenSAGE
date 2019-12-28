using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Mathematics.FixedMath;
using Priority_Queue;

namespace OpenSage.Navigation
{
    public enum Passability
    {
        Passable,
        Impassable,
        ImpassableForTeams,
        ImpassableForAir,
    }

    public class Node : FastPriorityQueueNode
    {
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

        internal Fix64 EstimateDistance(Node target)
        {
            var two = new Fix64(2);
            var xLength = Fix64.Pow(new Fix64(Math.Abs(target.X - X)), two);
            var yLength = Fix64.Pow(new Fix64(Math.Abs(target.Y - Y)), two);
            return Fix64.Sqrt(xLength + yLength);
        }

        internal IEnumerable<(Node, Fix64)> GetAdjacentPassableNodes()
        {
            return GetAdjacentNodes().Where(x => x.Item1.IsPassable);
        }

        private static readonly Fix64 SqrtOf2 = Fix64.Sqrt(new Fix64(2));

        internal IEnumerable<(Node, Fix64)> GetAdjacentNodes()
        {
            // Top
            if (Y > 0)
            {
                yield return (_graph.GetNode(X, Y - 1), Fix64.One);
            }
            // Top left
            if ( X > 0 && Y > 0)
            {
                yield return (_graph.GetNode(X - 1, Y - 1), SqrtOf2);
            }
            // Left
            if (X > 0)
            {
                yield return (_graph.GetNode(X - 1, Y), Fix64.One);
            }
            // Bottom left
            if (X > 0 && Y < _graph.Height - 1)
            {
                yield return (_graph.GetNode(X - 1, Y + 1), SqrtOf2);
            }
            // Bottom
            if (Y < _graph.Height - 1)
            {
                yield return (_graph.GetNode(X, Y + 1), Fix64.One);
            }
            // Bottom Right
            if (X < _graph.Width - 1 && Y < _graph.Height - 1)
            {
                yield return (_graph.GetNode(X + 1, Y + 1), SqrtOf2);
            }
            // Right
            if (X < _graph.Width - 1)
            {
                yield return (_graph.GetNode(X + 1, Y), Fix64.One);
            }
            // Top right
            if (X < _graph.Width - 1 && Y > 0)
            {
                yield return (_graph.GetNode(X + 1, Y - 1), SqrtOf2);
            }
        }
    }
}
