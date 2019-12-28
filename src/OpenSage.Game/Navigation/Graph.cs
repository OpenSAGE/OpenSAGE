using System.Collections.Generic;
using OpenSage.Mathematics.FixedMath;
using Priority_Queue;

namespace OpenSage.Navigation
{
    public class Graph
    {
        readonly Node[,] _nodes;

        public int Width => _nodes.GetLength(0);
        public int Height => _nodes.GetLength(1);


        public Node GetNode(int x, int y)
        {
            return _nodes[x, y];
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
            var cameFrom = new Dictionary<Node, Node>();
            var shortestKnownDistance = new Dictionary<Node, Fix64>();
            cameFrom[start] = start;
            shortestKnownDistance[start] = Fix64.Zero;
            
            var frontier = new FastPriorityQueue<Node>(10000);
            frontier.Enqueue(start, 0);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current == end)
                {
                    break;
                }

                var distanceToCurrent = shortestKnownDistance[current];

                foreach (var (next, cost) in current.GetAdjacentPassableNodes())
                {
                    var costToNext = distanceToCurrent + cost;

                    if(!shortestKnownDistance.TryGetValue(next, out var oldCost) || costToNext < oldCost)
                    {
                        shortestKnownDistance[next] = costToNext;
                        cameFrom[next] = current;
                        // Include distance from start & end as a cost.
                        var estimatedCostToEnd = costToNext + next.EstimateDistance(end);
                        frontier.Enqueue(next, (float) estimatedCostToEnd);
                    }
                }
            }

            return GetPath(cameFrom,start,end);
        }
    }
}
