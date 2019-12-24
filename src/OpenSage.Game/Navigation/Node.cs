using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Navigation
{
    enum Passability
    {
        Passable,
        Impassable,
        ImpassableForTeams,
        ImpassableForAir,
    }

    class Node
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

        internal bool Passable()
        {
            return Passability == Passability.Passable;
        }

        internal int CalculateDistance(Node target)
        {
            return Math.Abs(target.X - X) + Math.Abs(target.Y - Y);
        }

        internal IEnumerable<Node> GetAdjacentPassableNodes()
        {
            return GetAdjacentNodes().Where(x => x.Passable());
        }

        internal IEnumerable<Node> GetAdjacentNodes()
        {
            // Top
            if (Y > 0)
            {
                yield return _graph.GetNode(X, Y - 1);
            }
            // Top left
            if ( X > 0 && Y > 0)
            {
                yield return _graph.GetNode(X - 1, Y - 1);
            }
            // Left
            if (X > 0)
            {
                yield return _graph.GetNode(X - 1, Y);
            }
            // Bottom left
            if (X > 0 && Y < _graph.Height - 1)
            {
                yield return _graph.GetNode(X - 1, Y + 1);
            }
            // Bottom
            if (Y < _graph.Height - 1)
            {
                yield return _graph.GetNode(X, Y + 1);
            }
            // Bottom Right
            if (X < _graph.Width - 1 && Y < _graph.Height - 1)
            {
                yield return _graph.GetNode(X + 1, Y + 1);
            }
            // Right
            if (X < _graph.Width - 1)
            {
                yield return _graph.GetNode(X + 1, Y);
            }
            // Top right
            if (X < _graph.Width - 1 && Y > 0)
            {
                yield return _graph.GetNode(X + 1, Y - 1);
            }
        }
    }
}
