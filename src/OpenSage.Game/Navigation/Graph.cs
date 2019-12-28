using System.Collections.Generic;
using Priority_Queue;

namespace OpenSage.Navigation
{
    public class Graph
    {
        readonly Node[,] _nodes;

        public int Width => _nodes.GetLength(0);
        public int Height => _nodes.GetLength(1);

        private readonly FastPriorityQueue<Node> _unexpandedNodes;
        private readonly List<(Node, int)> _adjacentNodes;
        private readonly HashSet<Node> _visited;

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

            _unexpandedNodes = new FastPriorityQueue<Node>(w * h);
            _adjacentNodes = new List<(Node, int)>(8);
            _visited = new HashSet<Node>();
        }

        private List<Node> GetPath(Node start, Node end)
        {
            var result = new List<Node> { end };
            var iter = end.CameFrom;

            while (iter != start)
            {
                result.Add(iter);
                iter = iter.CameFrom;
            }

            result.Reverse();

            return result;
        }

        // A* pathfinding
        public List<Node> Search(Node start, Node end)
        {
            _unexpandedNodes.Clear();
            _visited.Clear();

            start.CameFrom = start;
            start.ShortestKnownDistance = 0;
            _unexpandedNodes.Enqueue(start, 0);

            while (_unexpandedNodes.Count > 0)
            {
                var current = _unexpandedNodes.Dequeue();
                _visited.Add(current);
                _unexpandedNodes.ResetNode(current);

                if (current == end)
                {
                    return GetPath(start, end);
                }

                var distanceToCurrent = current.ShortestKnownDistance;

                _adjacentNodes.Clear();
                current.GetAdjacentNodes(_adjacentNodes);

                foreach (var (next, cost) in _adjacentNodes)
                {
                    if (_visited.Contains(next))
                    {
                        continue;
                    }

                    var distanceToNext = distanceToCurrent + cost;
                    var estimatedDistanceToEnd = distanceToNext + next.EstimateDistance(end);

                    if (_unexpandedNodes.Contains(next))
                    {
                        if (distanceToNext < next.ShortestKnownDistance)
                        {
                            next.ShortestKnownDistance = distanceToNext;
                            _unexpandedNodes.UpdatePriority(next, estimatedDistanceToEnd);
                            next.CameFrom = current;
                        }
                    }
                    else
                    {
                        next.ShortestKnownDistance = distanceToNext;
                        _unexpandedNodes.Enqueue(next, estimatedDistanceToEnd);
                        next.CameFrom = current;
                    }
                }
            }

            return null;
        }
    }
}
