using OpenSage.Navigation;
using Xunit;

namespace OpenSage.Tests.Navigation
{
    public class PathOptimizerTest
    {
        [Fact]
        public void RedundantNodeRemoval()
        {
            var graph = new Graph(100, 100);
            var start = graph.GetNode(10, 10);
            var end = graph.GetNode(90, 90);

            var result = graph.Search(start, end);
            PathOptimizer.RemoveRedundantNodes(result);
            
            Assert.Equal(2, result.Count);
        }
    }
}
