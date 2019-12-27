using OpenSage.Navigation;
using Xunit;

namespace OpenSage.Tests.Navigation
{
    public class GraphTest
    {
        [Fact]
        public void NoObstacles()
        {
            var graph = new Graph(100, 100);
            var start = graph.GetNode(10, 10);
            var end = graph.GetNode(90, 90);

            var result = graph.Search(start, end);
            Assert.Equal(80, result.Count);
        }

        [Fact]
        public void WithObstacles()
        {
            var graph = new Graph(100, 100);
            var start = graph.GetNode(10, 10);
            var end = graph.GetNode(90, 90);

            for(int i=0;i<99;i++)
            {
                graph.GetNode(50, i).Passability = Passability.Impassable;
            }

            var result = graph.Search(start, end);
            Assert.Equal(129, result.Count);
        }

        [Fact]
        public void Impassable()
        {
            var graph = new Graph(100, 100);
            var start = graph.GetNode(10, 10);
            var end = graph.GetNode(90, 90);

            for (int i = 0; i < 100; i++)
            {
                graph.GetNode(50, i).Passability = Passability.Impassable;
            }

            var result = graph.Search(start, end);
            Assert.Null(result);
        }
    }
}
