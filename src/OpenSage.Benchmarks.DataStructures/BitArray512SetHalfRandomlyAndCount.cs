using System;
using System.Collections;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Benchmarks.DataStructures
{
    public class BitArray512SetHalfRandomlyAndCount
    {
        private readonly int _flagCount = Enum.GetValues(typeof(ModelConditionFlag)).Length;

        private readonly Random _random;
        private List<int> _locations;

        public BitArray512SetHalfRandomlyAndCount()
        {
            _random = new Random();
        }

        [GlobalSetup]
        public void GenerateData()
        {
            _locations = new List<int>();

            for (var i = 0; i < _flagCount; i++)
            {
                if (_random.NextDouble() > 0.5)
                {
                    _locations.Add(i);
                }
            }
        }


        [Benchmark(Baseline = true)]
        public int SetHalfRandomlyAndCountBCL()
        {
            var array = new BitArray(_flagCount);

            for (var i = 0; i < _locations.Count; i++)
            {
                array[_locations[i]] = true;
            }

            var count = 0;

            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == true)
                {
                    count++;
                }
            }

            return count;
        }

        [Benchmark]
        public int SetHalfRandomlyAndCountBitArray512()
        {
            var array = new BitArray512<ModelConditionFlag>();

            for (var i = 0; i < _locations.Count; i++)
            {
                array.Set(_locations[i], true);
            }

            return array.NumBitsSet;
        }
    }
}
