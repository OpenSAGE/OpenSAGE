using System;
using System.Collections;
using BenchmarkDotNet.Attributes;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Benchmarks.DataStructures
{

    public class BitArray512CheckExisting
    {
        private readonly int _flagCount = Enum.GetValues(typeof(ModelConditionFlag)).Length;

        private BitArray _bitArray;
        private BitArray512 _bitArray512;

        [GlobalSetup]
        public void GenerateData()
        {
            _bitArray = new BitArray(_flagCount);
            _bitArray512 = new BitArray512(_flagCount);

            _bitArray.Set(240, true);
            _bitArray512.Set(240, true);
        }


        [Benchmark(Baseline = true)]
        public bool CheckExistingBCL()
        {
            return _bitArray.Get(240);
        }

        [Benchmark]
        public bool CheckExistingBitAray512()
        {
            return _bitArray.Get(240);
        }
    }
}
