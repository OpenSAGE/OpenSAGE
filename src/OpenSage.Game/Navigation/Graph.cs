using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSage.Navigation
{
    class Graph
    {
        Node[,] _nodes;

        public int Width => _nodes.GetLength(0);
        public int Height => _nodes.GetLength(1);


        internal Node GetNode(int x, int y)
        {
            return _nodes[x, y];
        }

        internal Graph(int w, int h)
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

        public List<Node> Search(Node start, Node end)
        {
            var came_from = new Dictionary<Node,Node>();
            var frontier = new Queue<Node>();
            frontier.Enqueue(start);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current == end)
                    break;

                foreach (var next in current.GetAdjacentPassableNodes())
                {
                    if(!came_from.ContainsKey(next))
                    {
                        came_from[next] = current;
                        frontier.Enqueue(next);
                    }
                }
            }

            return GetPath(came_from,start,end);
        }
    }
}
