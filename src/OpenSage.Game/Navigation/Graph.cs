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
    }
}
