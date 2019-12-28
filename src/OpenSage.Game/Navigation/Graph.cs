using System.Collections.Generic;
using Priority_Queue;

namespace OpenSage.Navigation
{
    public class Graph
    {
        readonly Node[,] _nodes;

        public int Width => _nodes.GetLength(0);
        public int Height => _nodes.GetLength(1);

        private readonly Dictionary<Node, Node> _cameFrom;
        private readonly Dictionary<Node, int> _shortestKnownDistance;
        private readonly FastPriorityQueue<Node> _unexpandedNodes;
        private readonly List<(Node, int)> _adjacentNodes;

        public Node GetNode(int x, int y)
        {
            return _nodes[x, y];
        }

        public bool TryGetNode(int x, int y, out Node node)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                node = _nodes[x, y];
                return true;
            }

            node = default;
            return false;
        }

        public Graph(int w, int h)
        {
            _nodes = new Node[w, h];
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    _nodes[x, y] = new Node(this, x, y);
                }
            }

            _cameFrom = new Dictionary<Node, Node>();
            _shortestKnownDistance = new Dictionary<Node, int>();
            _unexpandedNodes = new FastPriorityQueue<Node>(w * h);
            _adjacentNodes = new List<(Node, int)>(8);
        }

        private List<Node> GetPath(Dictionary<Node, Node> paths, Node start, Node end)
        {
            if (!paths.ContainsKey(end))
            {
                return null;
            }

            var result = new List<Node> { end };
            var iter = paths[end];

            while(iter != start)
            {
                result.Add(iter);
                iter = paths[iter];
            }

            result.Reverse();

            return result;
        }

        // A* pathfinding
        public List<Node> Search(Node start, Node end)
        {
            _cameFrom.Clear();
            _shortestKnownDistance.Clear();
            _unexpandedNodes.Clear();
            _adjacentNodes.Clear();

            _cameFrom[start] = start;
            _shortestKnownDistance[start] = 0;
            _unexpandedNodes.Enqueue(start, 0);

            while (_unexpandedNodes.Count > 0)
            {
                var current = _unexpandedNodes.Dequeue();

                if (current == end)
                {
                    break;
                }

                var distanceToCurrent = _shortestKnownDistance[current];
                current.GetAdjacentNodes(_adjacentNodes);

                foreach (var (next, cost) in _adjacentNodes)
                {
                    var costToNext = distanceToCurrent + cost;

                    if(!_shortestKnownDistance.TryGetValue(next, out var oldCost) || costToNext < oldCost)
                    {
                        _shortestKnownDistance[next] = costToNext;
                        _cameFrom[next] = current;
                        // Include distance from start & end as a cost.
                        var estimatedCostToEnd = costToNext + next.EstimateDistance(end);
                        _unexpandedNodes.Enqueue(next, estimatedCostToEnd);
                    }
                }
            }

            return GetPath(_cameFrom, start, end);
        }
    }
}
