using System.Numerics;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Mathematics
{
    public class RectangleFTests
    {
        [Theory]
        [InlineData(0, 0, 1, 1, true)]
        [InlineData(1, 1, 4, 4, true)]
        [InlineData(0, 0, 5, 5, true)]
        [InlineData(-1, -1, 0.99f, 0.99f, false)]
        [InlineData(-1, -1, 1, 1, true)]
        [InlineData(5, 5, 1, 1, true)]
        [InlineData(5.01f, 5.01f, 1, 1, false)]
        [InlineData(5.01f, 0, 1, 1, false)]
        [InlineData(2, 2, 4, 4, true)]
        public void Intersects(float x, float y, float width, float height, bool expected)
        {
            var rect = new RectangleF(0, 0, 5, 5);
            Assert.Equal(expected, rect.Intersects(new RectangleF(x, y, width, height)));
        }

        [Theory]
        [InlineData(0, 0, 1, true)]
        [InlineData(-1, -1, 1, false)]
        [InlineData(6, 6, 1, false)]
        [InlineData(2, -1.01f, 1, false)]
        [InlineData(2, -1, 1, true)]
        [InlineData(2, 2, 1, true)]
        [InlineData(2.5f, 2.5f, 2.5f, true)]
        [InlineData(2.5f, 2.5f, 4, true)]
        [InlineData(2, 6, 1.1f, true)]
        [InlineData(6, 3, 1, true)]
        public void IntersectsCircle(float x, float y, float radius, bool expected)
        {
            var rect = new RectangleF(0, 0, 5, 5);
            Assert.Equal(expected, rect.Intersects(new Vector2(x, y), radius));
        }
    }
}
