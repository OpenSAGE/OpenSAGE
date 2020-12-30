using System.Numerics;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Mathematics
{
    public class TransformedRectangleTest
    {
        [Fact]
        public void IdenticalRectanglesIntersect()
        {
            var a = TransformedRectangle.FromRectangle(new RectangleF(0, 0, 10, 10));
            var b = a;
            Assert.True(a.Intersects(b));
        }

        [Fact]
        public void RotatedRectanglesIntersect()
        {
            var a = TransformedRectangle.FromRectangle(new RectangleF(0, 0, 10, 10), 3.2f);
            var b = TransformedRectangle.FromRectangle(new RectangleF(0, 0, 10, 10), 1.2f);
            Assert.True(a.Intersects(b));
        }

        [Fact]
        public void LargerRectangleContainsSmallerOne()
        {
            var a = TransformedRectangle.FromRectangle(new RectangleF(0, 0, 10, 10));
            var b = TransformedRectangle.FromRectangle(new RectangleF(-5, -5, 15, 15));
            Assert.True(a.Intersects(b));
        }

        [Fact]
        public void CompletelySeparateRectanglesDontIntersect()
        {
            var a = TransformedRectangle.FromRectangle(new RectangleF(0, 0, 1, 1));
            var b = TransformedRectangle.FromRectangle(new RectangleF(2, 0, 1, 1));
            Assert.False(a.Intersects(b));
        }

        [Theory]
        [InlineData(0, 0, 1, false)]
        [InlineData(-1, -1, 1, false)]
        [InlineData(6, 6, 1, false)]
        [InlineData(2, -1.01f, 1, false)]
        [InlineData(2, -1, 1, false)]
        [InlineData(5, 0, 1, false)]
        [InlineData(3, 3, 5, true)]
        public void IntersectsCircle(float x, float y, float radius, bool expected)
        {
            var rect = TransformedRectangle.FromRectangle(new RectangleF(0, 0, 5, 5), 0.75f);
            Assert.Equal(expected, rect.Intersects(new Vector2(x, y), radius));
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(2, 0, true)]
        [InlineData(4, 0, false)]
        [InlineData(4, 4, false)]
        [InlineData(0, 4, false)]
        [InlineData(2, 2, true)]
        [InlineData(0, 2, true)]
        [InlineData(2.1f, 2, true)]
        public void Contains(float x, float y, bool expected)
        {
            var sut = TransformedRectangle.FromRectangle(new RectangleF(0, 0, 4, 4), 0.75f);
            Assert.Equal(expected, sut.Contains(new Vector2(x, y)));
        }
    }
}
