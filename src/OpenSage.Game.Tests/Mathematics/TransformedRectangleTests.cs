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
    }
}
