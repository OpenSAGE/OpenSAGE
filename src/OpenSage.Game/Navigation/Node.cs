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
        private readonly int _x, _y;
        private readonly Graph _graph;
        public Passability Passability { get; set; }

        internal Node(Graph graph, int x, int y)
        {
            _x = x;
            _y = y;
            _graph = graph;
            Passability = Passability.Passable;
        }

        internal bool Passable()
        {
            return Passability == Passability.Passable;
        }

        internal int CalculateDistance(int targetX, int targetY)
        {
            return Math.Abs(targetX - _x) + Math.Abs(targetY - _y);
        }

        internal IEnumerable<Node> GetAdjacentPassableNodes()
        {
            return GetAdjacentNodes().Where(x => x.Passable());
        }

        internal IEnumerable<Node> GetAdjacentNodes()
        {
            if (_x > 0)
            {
                yield return _graph.GetNode(_x - 1, _y);
            }
            if (_y > 0)
            {
                yield return _graph.GetNode(_x, _y - 1);
            }
            if (_x < _graph.Width - 1)
            {
                yield return _graph.GetNode(_x + 1, _y);
            }
            if (_y < _graph.Height - 1)
            {
                yield return _graph.GetNode(_x, _y + 1);
            }
        }
    }
}
