using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using OpenSage.DataStructures;
using OpenSage.Mathematics;

namespace OpenSage.Benchmarks.DataStructures
{
    public class QuadtreeInsert
    {
        private readonly Random _random;
        private readonly RectangleF _treeBounds;

        private List<BenchQuadtreeItem> _items;

        public QuadtreeInsert()
        {
            _random = new Random();
            _treeBounds = new RectangleF(0, 0, 1000, 1000);
        }

        [Params(15)]
        public float MaxItemSize { get; set; }

        [Params(5000)]
        public int InsertedItems { get; set; }

        [GlobalSetup]
        public void GenerateData()
        {
            _items = new List<BenchQuadtreeItem>(InsertedItems);

            for (var i = 0; i < InsertedItems; i++)
            {
                _items.Add(BenchQuadtreeItem.Generate(i, _treeBounds, new SizeF(MaxItemSize), _random));
            }
        }

        [Benchmark(Baseline = true)]
        public List<BenchQuadtreeItem> InsertList()
        {
            var items = new List<BenchQuadtreeItem>();

            foreach (var item in _items)
            {
                items.Add(item);
            }

            return items;
        }

        [Benchmark(OperationsPerInvoke = 100)]
        public Quadtree<BenchQuadtreeItem> InsertTest()
        {
            var tree = new Quadtree<BenchQuadtreeItem>(_treeBounds);

            foreach (var item in _items)
            {
                tree.Insert(item);
            }

            return tree;
        }
    }
}
