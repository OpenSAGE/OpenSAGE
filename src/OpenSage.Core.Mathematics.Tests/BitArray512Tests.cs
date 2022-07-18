using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Mathematics
{
    public class BitArray512Tests
    {
        [Fact]
        public void CanSetAndUnsetAll()
        {
            var arr = new BitArray512(512);

            Assert.Equal(0, arr.NumBitsSet);

            for (var i = 0; i < arr.Length; i++)
            {
                arr.Set(i, true);
                Assert.True(arr.Get(i));
                arr.Set(i, false);
                Assert.False(arr.Get(i));
            }

            Assert.Equal(0, arr.NumBitsSet);
        }

        [Fact]
        public void CanClearIndividual()
        {
            var arr = new BitArray512(230);
            arr.SetAll(true);
            arr.Set(123, false);
            Assert.False(arr.Get(123));
        }

        [Fact]
        public void SetOneCount()
        {
            var arr = new BitArray512(230);
            arr.Set(37, true);
            Assert.Equal(1, arr.NumBitsSet);
        }

        [Fact]
        public void SetBitCountUpdates()
        {
            var arr = new BitArray512(107);
            arr.Set(100, true);
            Assert.Equal(1, arr.NumBitsSet);
            arr.Set(7, true);
            Assert.Equal(2, arr.NumBitsSet);
            arr.Set(100, false);
            Assert.Equal(1, arr.NumBitsSet);
            arr.Set(7, false);
            Assert.Equal(0, arr.NumBitsSet);
        }

        [Fact]
        public void SetOneInEveryLongCount()
        {
            var arr = new BitArray512(512);
            arr.Set(0  * 0, true);
            arr.Set(64 * 1, true);
            arr.Set(64 * 2, true);
            arr.Set(64 * 3, true);
            arr.Set(64 * 4, true);
            arr.Set(64 * 5, true);
            arr.Set(64 * 6, true);
            arr.Set(64 * 7, true);
            Assert.Equal(8, arr.NumBitsSet);
        }

        [Fact]
        public void CountAfterSetAllReturnsMaxCount()
        {
            var arr = new BitArray512(510);
            arr.SetAll(true);
            Assert.Equal(arr.Length, arr.NumBitsSet);
        }

        [Fact]
        public void SetAllClearAllZeroCount()
        {
            var arr = new BitArray512(510);
            arr.SetAll(true);
            arr.SetAll(false);
            Assert.Equal(0, arr.NumBitsSet);
        }

        [Fact]
        public void CanSetAllBitsIndividually()
        {
            var arr = new BitArray512(117);

            for (var i = 0; i < arr.Length; i++)
            {
                arr.Set(i, true);
                Assert.Equal(i + 1, arr.NumBitsSet);
            }

            Assert.Equal(arr.Length, arr.NumBitsSet);
        }

        [Fact]
        public void CanClearAllBitsIndividually()
        {
            var arr = new BitArray512(510);
            arr.SetAll(true);

            for (var i = 0; i < arr.Length; i++)
            {
                arr.Set(i, false);
                Assert.Equal(arr.Length - i - 1, arr.NumBitsSet);
            }

            Assert.Equal(0, arr.NumBitsSet);
        }

        [Fact]
        public void AndSmoke()
        {
            var arr1 = new BitArray512(100);
            arr1.Set(87, true);

            var arr2 = new BitArray512(100);
            arr2.Set(87, true);

            var arr3 = arr1.And(arr2);
            Assert.Equal(1, arr3.NumBitsSet);

            for (var i = 0; i < arr3.Length; i++)
            {
                if (i == 87)
                {
                    Assert.True(arr3.Get(i));
                }
                else
                {
                    Assert.False(arr3.Get(i));
                }
            }
        }

        [Fact]
        public void AndWithEmptyIsEmpty()
        {
            var arr1 = new BitArray512(350);
            arr1.Set(1, true);
            arr1.Set(87, true);
            arr1.Set(300, true);

            var arr2 = new BitArray512(350);

            var arr3 = arr1.And(arr2);
            Assert.False(arr3.Get(1));
            Assert.False(arr3.Get(87));
            Assert.False(arr3.Get(300));
            Assert.Equal(0, arr3.NumBitsSet);
        }
    }
}
