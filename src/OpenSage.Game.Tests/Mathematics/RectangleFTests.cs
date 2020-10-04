using System.Numerics;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Mathematics
{
    public class RectangleFTests
    {
        [Theory]
        [InlineData(0, 0, 1, true)]
        [InlineData(-1, -1, 1, false)]
        [InlineData(6, 6, 1, false)]
        [InlineData(2, -1.01f, 1, false)]
        [InlineData(2, -1, 1, true)]
        public void IntersectsCircle(float x, float y, float radius, bool expected)
        {
            var rect = new RectangleF(0, 0, 5, 5);
            Assert.Equal(expected, rect.Intersects(new Vector2(x, y), radius));
        }
    }
}
