using System;
using System.Collections;
using BenchmarkDotNet.Attributes;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Benchmarks.DataStructures
{
    public class BitArray512SetAllExceptOneAndCount
    {
        private readonly int _flagCount = Enum.GetValues(typeof(ModelConditionFlag)).Length;

        [Benchmark(Baseline = true)]
        public int SetAllExceptOneAndCountBCL()
        {
            var array = new BitArray(_flagCount);
            array.SetAll(true);
            array.Set(123, false);

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
        public int SetAllExceptOneAndCountBitArray512()
        {
            var array = new BitArray512(_flagCount);
            array.SetAll(true);
            array.Set(123, false);
            return array.NumBitsSet;
        }
    }
}
