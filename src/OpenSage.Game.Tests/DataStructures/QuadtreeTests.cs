using System;
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
        public class MockQuadtreeItem : ICollidable
        {
            public readonly int Id;
            public Collider Collider { get; set; }

            public Collider RoughCollider { get; }

            public List<Collider> Colliders { get; }

            public Vector3 Translation => throw new System.NotImplementedException();

            public MockQuadtreeItem(int id, RectangleF bounds)
            {
                Id = id;
                var collider = new BoxCollider(bounds);
                RoughCollider = collider;
                Colliders = new List<Collider> { collider };
            }

            // implementation copied from GameObject
            public bool CollidesWith(ICollidable other, bool twoDimensional)
            {
                if (RoughCollider == null || other.RoughCollider == null)
                {
                    return false;
                }

                if (!RoughCollider.Intersects(other.RoughCollider, twoDimensional))
                {
                    return false;
                }

                foreach (var collider in Colliders)
                {
                    foreach (var otherCollider in other.Colliders)
                    {
                        if (collider.Intersects(otherCollider, twoDimensional))
                        {
                            return true;
                        }
                    }
                }
                return false;
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

        #region Searcher

        [Fact]
        public void SearcherDoesNotIntersectItself()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(60, 60, 2, 2));

            quadtree.Insert(item);

            Assert.Empty(quadtree.FindIntersecting(item));
        }

        [Fact]
        public void SearcherDoesNotIntersectOthersNearby()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(60, 60, 2, 2));

            quadtree.Insert(item);

            var item2 = new MockQuadtreeItem(2, new RectangleF(60, 57, 2, 2));

            quadtree.Insert(item2);

            Assert.Empty(quadtree.FindIntersecting(item));
        }

        [Fact]
        public void SearcherIntersectsOthers()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(60, 60, 2, 2));

            quadtree.Insert(item);

            var item2 = new MockQuadtreeItem(2, new RectangleF(60, 59, 2, 2));

            quadtree.Insert(item2);

            Assert.Equal(new[] { item2 }, quadtree.FindIntersecting(item));
        }

        [Fact]
        public void SearcherTouchesOthersEdgeNoIntersection()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(60, 60, 2, 2));

            quadtree.Insert(item);

            var item2 = new MockQuadtreeItem(2, new RectangleF(60, 58, 2, 2));

            quadtree.Insert(item2);

            Assert.Empty(quadtree.FindIntersecting(item));
        }

        [Fact]
        public void SearcherTouchesOthersCornerNoIntersection()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(60, 60, 2, 2));

            quadtree.Insert(item);

            var item2 = new MockQuadtreeItem(2, new RectangleF(58, 58, 2, 2));

            quadtree.Insert(item2);

            Assert.Empty(quadtree.FindIntersecting(item));
        }

        [Fact]
        public void SearcherContainsOthers()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(59, 59, 3, 3));

            quadtree.Insert(item);

            var item2 = new MockQuadtreeItem(2, new RectangleF(60, 60, 1, 1));

            quadtree.Insert(item2);

            Assert.Equal(new[] { item2 }, quadtree.FindIntersecting(item));
        }

        [Fact]
        public void SearcherIsContainedByOthers()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(60, 60, 1, 1));

            quadtree.Insert(item);

            var item2 = new MockQuadtreeItem(2, new RectangleF(59, 59, 3, 3));

            quadtree.Insert(item2);

            Assert.Equal(new[] { item2 }, quadtree.FindIntersecting(item));
        }

        [Fact]
        public void SearcherIntersectsOthersOverlappingBoundary()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 10, 10));

            var item = new MockQuadtreeItem(1, new RectangleF(0, 0, 1, 1));
            var item2 = new MockQuadtreeItem(1, new RectangleF(4, 0, 2, 2));
            var item3 = new MockQuadtreeItem(1, new RectangleF(3, 0, 2, 2));
            var item4 = new MockQuadtreeItem(1, new RectangleF(5, 0, 2, 2));

            quadtree.Insert(item);
            quadtree.Insert(item2);
            quadtree.Insert(item3);
            quadtree.Insert(item4);

            Assert.Equal(new[] { item2 }, quadtree.FindIntersecting(item3));
            Assert.Equal(new[] { item2 }, quadtree.FindIntersecting(item4));
        }

        #endregion

        #region Bounding Box

        [Fact]
        public void RectangleDoesNotIntersectOthersNearby()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(60, 60, 2, 2));

            quadtree.Insert(item);

            Assert.Empty(quadtree.FindIntersecting(new BoxCollider(new RectangleF(60, 57, 2, 2))));
        }

        [Fact]
        public void RectangleIntersectsOthers()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(60, 60, 2, 2));

            quadtree.Insert(item);

            Assert.Equal(new[] { item }, quadtree.FindIntersecting(new BoxCollider(new RectangleF(60, 59, 2, 2))));
        }

        [Fact]
        public void RectangleTouchesOthersEdgeNoIntersection()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(60, 60, 2, 2));

            quadtree.Insert(item);

            Assert.Empty(quadtree.FindIntersecting(new BoxCollider(new RectangleF(60, 58, 2, 2))));
        }

        [Fact]
        public void RectangleTouchesOthersCornerNoIntersection()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(60, 60, 2, 2));

            quadtree.Insert(item);

            Assert.Empty(quadtree.FindIntersecting(new BoxCollider(new RectangleF(58, 58, 2, 2))));
        }

        [Fact]
        public void RectangleContainsOthers()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(60, 60, 1, 1));

            quadtree.Insert(item);

            Assert.Equal(new[] { item }, quadtree.FindIntersecting(new BoxCollider(new RectangleF(59, 59, 3, 3))));
        }

        [Fact]
        public void RectangleIsContainedByOthers()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 100, 100));

            var item = new MockQuadtreeItem(1, new RectangleF(59, 59, 3, 3));

            quadtree.Insert(item);

            Assert.Equal(new[] { item }, quadtree.FindIntersecting(new BoxCollider(new RectangleF(60, 60, 1, 1))));
        }

        [Fact]
        public void RectangleIntersectsOthersOverlappingBoundary()
        {
            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(0, 0, 10, 10));

            var item = new MockQuadtreeItem(1, new RectangleF(0, 0, 1, 1));
            var item2 = new MockQuadtreeItem(1, new RectangleF(0, 0, 1, 1));

            quadtree.Insert(item);
            quadtree.Insert(item2);

            var item3 = new MockQuadtreeItem(1, new RectangleF(4, 0, 2, 2));
            quadtree.Insert(item3);

            Assert.Equal(new[] { item3 }, quadtree.FindIntersecting(new BoxCollider(new RectangleF(5, 0, 2, 2))));
            Assert.Equal(new[] { item3 }, quadtree.FindIntersecting(new BoxCollider(new RectangleF(3, 0, 2, 2))));
        }

        #endregion

        // This tests a large number of items against the "dumb" way of checking for collisions to ensure the quadtree implementation is correct.
        [Fact]
        public void FuzzFindIntersecting()
        {
            const int itemsToGenerate = 1000;
            const float minX = 0f;
            const float minY = 0f;
            const float maxX = 200f;
            const float maxY = 150f;
            const int seed = 42; // fix seed to fuzz results are consistent

            var r = new Random(seed);

            var quadtree = new Quadtree<MockQuadtreeItem>(new RectangleF(minX, minY, maxX, maxY));

            var items = new List<MockQuadtreeItem>();

            for (var i = 0; i < itemsToGenerate; i++)
            {
                var iMinX = minX + r.NextDouble() * (maxX - minX);
                var iMinY = minY + r.NextDouble() * (maxY - minY);
                var width = r.NextDouble() * Math.Min(maxX - iMinX, maxX / 10);
                var height = r.NextDouble() * Math.Min(maxY - iMinY, maxY / 10);

                var item = new MockQuadtreeItem(i, new RectangleF((float)iMinX, (float)iMinY, (float)width, (float)height));
                items.Add(item);
                Assert.True(quadtree.Insert(item));
            }

            var collisions = new Dictionary<int, HashSet<int>>();

            // build a manual register of intersecting items to test the quadtree against
            foreach (var item1 in items)
            {
                if (!collisions.ContainsKey(item1.Id))
                {
                    collisions[item1.Id] = [];
                }

                foreach (var item2 in items.Where(item2 => item1.RoughCollider.Intersects(item2.RoughCollider)))
                {
                    if (item1.Id == item2.Id)
                    {
                        continue;
                    }

                    if (!collisions.ContainsKey(item2.Id))
                    {
                        collisions[item2.Id] = [];
                    }

                    collisions[item1.Id].Add(item2.Id);
                    collisions[item2.Id].Add(item1.Id);
                }
            }

            foreach (var i in items)
            {
                var results = quadtree.FindIntersecting(i).Select(item => item.Id).ToHashSet();
                Assert.Equal(collisions[i.Id].Count, results.Intersect(collisions[i.Id]).Count());
            }
        }
    }
}
