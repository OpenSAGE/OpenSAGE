using OpenSage.Navigation;
using Xunit;

namespace OpenSage.Tests.Navigation
{
    public class PathfindingTest
    {
        [Fact]
        void NoObstacles()
        {
            var graph = new Graph(100, 100);
            var start = graph.GetNode(10, 10);
            var end = graph.GetNode(90, 90);

            var result = graph.Search(start, end);
            Assert.Equal(80, result.Count);
        }
    }
}
