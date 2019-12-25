using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using OpenSage.Navigation;

namespace OpenSage.Benchmarks.DataStructures
{
    public class PathfindingQuery
    {

        Graph _graph;

        public PathfindingQuery()
        {
            _graph = new Graph(1000, 1000);
        }


        [Benchmark]
        public List<Node> DirectQuery()
        {
            var start = _graph.GetNode(10, 10);
            var end = _graph.GetNode(990, 990);
            return _graph.Search(start, end);
        }

    }
}
