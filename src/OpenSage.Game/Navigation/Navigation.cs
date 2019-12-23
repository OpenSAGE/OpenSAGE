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
            _graph = new Graph();
        }
    }
}
