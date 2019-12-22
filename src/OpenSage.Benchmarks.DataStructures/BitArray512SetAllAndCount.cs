using System;
using System.Collections;
using BenchmarkDotNet.Attributes;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Benchmarks.DataStructures
{
    public class BitArray512SetAllAndCount
    {
        private readonly int _flagCount = Enum.GetValues(typeof(ModelConditionFlag)).Length;

        [Benchmark(Baseline = true)]
        public int SetAllAndCountBCL()
        {
            var array = new BitArray(_flagCount);
            array.SetAll(true);

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
        public int SetAllAndCountBitArray512()
        {
            var array = new BitArray512(_flagCount);
            array.SetAll(true);
            return array.NumBitsSet;
        }
    }
}
