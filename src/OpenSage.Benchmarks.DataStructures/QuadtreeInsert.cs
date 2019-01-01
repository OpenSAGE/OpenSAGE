using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using OpenSage.DataStructures;
using OpenSage.Mathematics;

namespace OpenSage.Benchmarks.DataStructures
{
    public class BenchQuadtreeItem : IHasBounds
    {
        public readonly int Id;
        public RectangleF Bounds { get; }

        public BenchQuadtreeItem(int id, RectangleF bounds)
        {
            Id = id;
            Bounds = bounds;
        }

        public static BenchQuadtreeItem Generate(int id, RectangleF bounds, SizeF maxSize, Random random)
        {
            var x = (float) random.NextDouble() * bounds.Width - bounds.Left;
            var y = (float) random.NextDouble() * bounds.Height - bounds.Top;

            var width = MathF.Min(bounds.Right - x, (float) (random.NextDouble() * maxSize.Width));
            var height = MathF.Min(bounds.Bottom - y, (float) (random.NextDouble() * maxSize.Height));

            return new BenchQuadtreeItem(id, new RectangleF(x, y, width, height));
        }
    }

    public class QuadtreeInsert
    {
        private readonly Random _random;
        private readonly RectangleF _treeBounds;

        private List<BenchQuadtreeItem> _smallItems;

        public QuadtreeInsert()
        {
            _random = new Random();
            _treeBounds = new RectangleF(0, 0, 1000, 1000);
        }

        [Params(10)]
        public float MaxItemSize { get; set; }

        [Params(5000, 10000)]
        public int InsertedItems { get; set; }

        [GlobalSetup]
        public void GenerateData()
        {
            _smallItems = new List<BenchQuadtreeItem>(InsertedItems);

            for (var i = 0; i < InsertedItems; i++)
            {
                _smallItems.Add(BenchQuadtreeItem.Generate(i, _treeBounds, new SizeF(MaxItemSize), _random));
            }
        }

        [Benchmark]
        public void InsertTest()
        {
            var tree = new Quadtree<BenchQuadtreeItem>(_treeBounds);

            foreach (var item in _smallItems)
            {
                tree.Insert(item);
            }
        }
    }
}
