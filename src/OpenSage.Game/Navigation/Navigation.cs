using System;
using System.Collections.Generic;
using System.Text;
using OpenSage.Data.Map;

namespace OpenSage.Navigation
{
    class Navigation
    {
        Graph _graph;

        public Navigation(BlendTileData tileData)
        {
            var width = tileData.Impassability.GetLength(0);
            var height = tileData.Impassability.GetLength(1);
            _graph = new Graph(width, height);

            for (int x = 0; x < _graph.Width; x++)
            {
                for (int y = 0; y < _graph.Height; y++)
                {
                    bool passable = tileData.Impassability[x, y];
                    _graph.GetNode(x, y).Passability = passable ? Passability.Passable : Passability.Impassable;
                }
            }

            var route = _graph.Search(_graph.GetNode(5, 5), _graph.GetNode(20, 20));
        }
    }
}
