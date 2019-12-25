using System;
using System.Collections.Generic;
using System.Text;
using Priority_Queue;

namespace OpenSage.Navigation
{
    public class Graph
    {
        Node[,] _nodes;

        public int Width => _nodes.GetLength(0);
        public int Height => _nodes.GetLength(1);


        public Node GetNode(int x, int y)
        {
            return _nodes[x, y];
        }

        public Graph(int w, int h)
        {
            _nodes = new Node[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    _nodes[x, y] = new Node(this, x, y);
                }
            }
        }

        private List<Node> GetPath(Dictionary<Node, Node> paths, Node start, Node end)
        {
            var result = new List<Node>();
            result.Add(end);
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
            var came_from = new Dictionary<Node,Node>();
            var cost_so_far = new Dictionary<Node, int>();
            came_from[start] = start;
            cost_so_far[start] = 0;
            
            var frontier = new SimplePriorityQueue<Node>();
            frontier.Enqueue(start, 0);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current == end)
                    break;

                foreach (var next in current.GetAdjacentPassableNodes())
                {
                    int newCost = cost_so_far[current] + current.CalculateDistance(next);
                    if(!cost_so_far.ContainsKey(next) || newCost < cost_so_far[next])
                    {
                        cost_so_far[next] = newCost;
                        came_from[next] = current;
                        // Include distance from start & end as a cost.
                        int priority = newCost + next.CalculateDistance(end);
                        frontier.Enqueue(next, priority);
                    }
                }
            }

            return GetPath(came_from,start,end);
        }
    }
}
