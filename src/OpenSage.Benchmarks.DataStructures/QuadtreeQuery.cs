using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using OpenSage.DataStructures;
using OpenSage.Mathematics;

namespace OpenSage.Benchmarks.DataStructures
{
    public class QuadtreeQuery
    {
        private readonly Random _random;
        private Quadtree<BenchQuadtreeItem> _quadtree;
        private BenchQuadtreeItem[] _items;

        private RectangleF _queryArea;

        [Params(3000)]
        public int ItemCount { get; set; }

        [Params(10f)]
        public float ItemSize { get; set; }

        [Params(100f)]
        public float AreaSideLength { get; set; }

        public QuadtreeQuery()
        {
            _random = new Random();
        }

        [GlobalSetup]
        public void GenerateData()
        {
            _quadtree = new Quadtree<BenchQuadtreeItem>(new RectangleF(0, 0, 5000, 5000));
            _items = new BenchQuadtreeItem[ItemCount];

            for (var i = 0; i < ItemCount; i++)
            {
                var item = BenchQuadtreeItem.Generate(i, _quadtree.Bounds, new SizeF(ItemSize), _random);
                _quadtree.Insert(item);
                _items[i] = item;
            }

            // Since items are placed uniformly, it shouldn't matter which area this is.
            _queryArea = new RectangleF(500, 500, AreaSideLength, AreaSideLength);
        }

        [Benchmark(Baseline = true)]
        public List<BenchQuadtreeItem> QueryAreaArray()
        {
            var items = new List<BenchQuadtreeItem>();

            foreach (var item in _items)
            {
                if (item.Bounds.IntersectsWith(_queryArea))
                {
                    items.Add(item);
                }
            }

            return items;
        }

        [Benchmark]
        public List<BenchQuadtreeItem> QueryAreaQuadtree()
        {
            return _quadtree.FindIntersecting(_queryArea).ToList();
        }
    }
}