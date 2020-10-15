using System;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using OpenSage.DataStructures;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Benchmarks.DataStructures
{
    // Tests the performance of Quadtree.Update.
    // This is done by creating a 5000x5000 quadtree, and adding 3000 items to it.
    // 100 items are moved to a (precomputed) new position.
    public class QuadtreeUpdate
    {
        private readonly Random _random;
        private readonly RectangleF _bounds;
        private Quadtree<BenchQuadtreeItem> _quadtree;

        private BenchQuadtreeItem[] _items;
        private BenchQuadtreeItem[] _movingItemsOriginal;
        private BenchQuadtreeItem[] _movingItems;
        private RectangleF[] _newRects;

        public QuadtreeUpdate()
        {
            _random = new Random();
            _bounds = new RectangleF(0, 0, 5000, 5000);
        }

        [Params(15000)]
        public int TotalItems { get; set; }

        [Params(10000)]
        public int MovingItems { get; set; }

        private const float MaxItemSize = 20f;
        private const float MaximumMovementDistance = 20f;

        [GlobalSetup]
        public void GenerateData()
        {
            _items = new BenchQuadtreeItem[TotalItems];
            _movingItemsOriginal = new BenchQuadtreeItem[MovingItems];
            _newRects = new RectangleF[MovingItems];

            for (var i = 0; i < TotalItems; i++)
            {
                var item = BenchQuadtreeItem.Generate(i, _bounds, new SizeF(MaxItemSize), _random);

                _items[i] = item;

                if (i < MovingItems)
                {
                    var movementVector =
                        Vector2.Normalize(new Vector2((float) _random.NextDouble(), (float) _random.NextDouble())) *
                        (float) _random.NextDouble() * MaximumMovementDistance;

                    var newPosition = item.Collider.AxisAlignedBoundingArea.Position + movementVector;

                    newPosition =
                        Vector2.Max(
                            Vector2.Min(newPosition,
                                _bounds.Position + _bounds.Size.ToVector2() -
                                item.Collider.AxisAlignedBoundingArea.Size.ToVector2()), _bounds.Position);

                    _movingItemsOriginal[i] = item;
                    _newRects[i] = new RectangleF(newPosition, item.Collider.AxisAlignedBoundingArea.Size);
                }
            }
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _quadtree = new Quadtree<BenchQuadtreeItem>(_bounds);

            foreach (var item in _items)
            {
                _quadtree.Insert(item);
            }

            _movingItems = _movingItemsOriginal.Select(x => x.Clone()).ToArray();
        }

        [Benchmark(Baseline = true)]
        public void UpdateWithoutQuadtree()
        {
            for (var i = 0; i < MovingItems; i++)
            {
                _movingItems[i].Collider = new BoxCollider(_newRects[i]);
            }
        }

        [Benchmark]
        public void UpdateQuadtree()
        {
            for (var i = 0; i < MovingItems; i++)
            {
                var item = _movingItems[i];
                item.Collider = new BoxCollider(_newRects[i]);
                _quadtree.Update(item);
            }
        }
    }
}
