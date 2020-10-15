using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.DataStructures;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.DataStructures
{
    public class QuadtreeTests
    {
        public class MockQuadtreeItem : IHasCollider
        {
            public readonly int Id;
            public Collider Collider { get; set; }

            public MockQuadtreeItem(int id, RectangleF bounds)
            {
                Id = id;
                Collider = new BoxCollider(bounds);
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
        public void InsertAndRemoveManyOverlapping()
        {
            var items = new List<MockQuadtreeItem>
            {
                new MockQuadtreeItem(1, new RectangleF(0, 0, 1, 1)),
                new MockQuadtreeItem(2, new RectangleF(0, 0, 1.25f, 1.25f)),
                new MockQuadtreeItem(3, new RectangleF(0, 0, 1.5f, 1.5f)),
                new MockQuadtreeItem(4, new RectangleF(0, 0, 1.75f, 1.75f)),
                new MockQuadtreeItem(5, new RectangleF(0, 0, 2f, 2f)),
            };

            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 10, 10));

            foreach (var item in items)
            {
                quadtree.Insert(item);
            }

            Assert.Equal(5, quadtree.FindIntersecting(quadtree.Bounds).Count());

            foreach (var item in items)
            {
                quadtree.Remove(item);
            }

            Assert.Empty(quadtree.FindIntersecting(quadtree.Bounds));
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

            var actual = quadtree.FindIntersecting(quadtree.Bounds).OrderBy(x => x.Id);
            Assert.Equal(items, actual);
        }

        [Fact]
        public void InsertAndRemove()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 10, 10));

            var item = new MockQuadtreeItem(1, new RectangleF(0.5f, 0.5f, 1, 1));

            quadtree.Insert(item);
            quadtree.Remove(item);

            var result = quadtree.FindIntersecting(quadtree.Bounds);
            Assert.Empty(result);
        }

        [Fact]
        public void InsertAndRemoveNonExistant()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 10, 10));

            var item = new MockQuadtreeItem(1, new RectangleF(8, 8, 2, 2));

            quadtree.Insert(item);

            var notInsertedItem = new MockQuadtreeItem(1, new RectangleF(7, 7, 2, 2));

            quadtree.Remove(notInsertedItem);

            Assert.Equal(new[] {item}, quadtree.FindIntersecting(quadtree.Bounds));
        }
    }
}
