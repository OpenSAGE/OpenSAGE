using System.Numerics;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Mathematics
{
    public class DVector3Tests
    {
        [Fact]
        public void SumSmokeTest()
        {
            var a = new DVector3(1, 0, 0);
            var b = new DVector3(1, 1, 1);
            Assert.Equal(new DVector3(2, 1, 1), a + b);
        }

        [Fact]
        public void ToVector3Test()
        {
            var vec = new DVector3(1.234f, 2.345f, 3.456f);
            Assert.Equal(new Vector3(1.234f, 2.345f, 3.456f), vec.ToVector3());
        }

        [Fact]
        public void FromVector3Test()
        {
            var vec = new Vector3(100, 200, 300);
            Assert.Equal(new DVector3(100, 200, 300), DVector3.FromVector3(vec));
        }
    }
}
