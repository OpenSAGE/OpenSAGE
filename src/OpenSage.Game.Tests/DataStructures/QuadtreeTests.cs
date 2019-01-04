using System.Linq;
using OpenSage.DataStructures;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.DataStructures
{
    public class QuadtreeTests
    {
        public class MockQuadtreeItem : IHasBounds
        {
            public readonly int Id;
            public RectangleF Bounds { get; }

            public MockQuadtreeItem(int id, RectangleF bounds)
            {
                Id = id;
                Bounds = bounds;
            }
        }

        [Fact]
        public void CreateEmpty()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));
            Assert.Empty(quadtree.FindIntersecting(quadtree.Bounds));
        }

        [Fact]
        public void CreateAndInsertUnderLimit()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            quadtree.Insert(new MockQuadtreeItem(1, new RectangleF(0, 0, 1, 1)));
            quadtree.Insert(new MockQuadtreeItem(2, new RectangleF(1, 1, 2, 2)));

            var allItems = quadtree.FindIntersecting(quadtree.Bounds).ToList();
            Assert.Equal(2, allItems.Count);
        }

        [Fact]
        public void CreateAndInsertOverLimit()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 10, 10));

            var items = new[]
            {
                new MockQuadtreeItem(1, new RectangleF(0, 0, 1, 1)),
                new MockQuadtreeItem(2, new RectangleF(1, 2, 1, 1)),
                new MockQuadtreeItem(3, new RectangleF(9, 9, 1, 1)),
                new MockQuadtreeItem(4, new RectangleF(4, 4, 2, 2)),
                new MockQuadtreeItem(5, new RectangleF(3, 3, 1, 1))
            };

            foreach (var item in items)
            {
                quadtree.Insert(item);
            }

            Assert.Equal(items, quadtree.FindIntersecting(quadtree.Bounds).OrderBy(x => x.Id));
        }

        [Fact]
        public void InsertAndRemove()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 10, 10));

            var item = new MockQuadtreeItem(1, new RectangleF(0.5f, 0.5f, 1, 1));

            quadtree.Insert(item);
            quadtree.Remove(item.Bounds);

            Assert.Empty(quadtree.FindIntersecting(quadtree.Bounds));
        }

        [Fact]
        public void InsertAndRemoveNonExistant()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 10, 10));

            var item = new MockQuadtreeItem(1, new RectangleF(8, 8, 2, 2));

            quadtree.Insert(item);
            quadtree.Remove(new RectangleF(0, 0, 1, 1));

            Assert.Equal(new[] {item}, quadtree.FindIntersecting(quadtree.Bounds));
        }
    }
}
