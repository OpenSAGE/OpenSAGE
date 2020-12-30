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
            var sut = new RectangleF(0, 0, 5, 5);
            Assert.Equal(expected, sut.Intersects(new RectangleF(x, y, width, height)));
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
            var sut = new RectangleF(0, 0, 5, 5);
            Assert.Equal(expected, sut.Intersects(new Vector2(x, y), radius));
        }

        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(2, 1, 1)]
        [InlineData(5, 5, 5)]
        [InlineData(2, 2, -1)]
        public void Scale(float width, float height, float scale)
        {
            var sut = new RectangleF(0, 0, width, height);

            sut = RectangleF.Scale(sut, scale);

            Assert.Equal(scale * width, sut.Width);
            Assert.Equal(scale * height, sut.Height);
        }

        [Theory]
        [InlineData(0.5f, 0.5f, true)]
        [InlineData(0, 0, true)]
        [InlineData(1, 1, true)]
        [InlineData(1, 0, true)]
        [InlineData(0, 1, true)]
        [InlineData(-0.01f, 0, false)]
        [InlineData(0, -0.01f, false)]
        [InlineData(1.01f, 0, false)]
        [InlineData(0, 1.01f, false)]
        [InlineData(0.5f, 0, true)]
        public void ContainsPoint(float x, float y, bool expected)
        {
            var sut = new RectangleF(0, 0, 1, 1);
            Assert.Equal(expected, sut.Contains(x, y));
        }

        [Theory]
        [InlineData(0, 0, 1, 1, true)]
        [InlineData(0.1f, 0.1f, 0.9f, 0.9f, true)]
        [InlineData(-0.1f, 0, 1, 1, false)]
        [InlineData(1, 1, 1, 1, false)]
        [InlineData(1, 0, 1, 1, false)]
        [InlineData(-1, 0, 1, 1, false)]
        [InlineData(-1, -1, 3, 3, false)]
        [InlineData(-0.5f, -0.5f, 1, 1, false)]
        public void ContainsRectF(float x, float y, float width, float height, bool expected)
        {
            var sut = new RectangleF(0, 0, 1, 1);
            Assert.Equal(expected, sut.Contains(new RectangleF(x, y, width, height)));
        }

        [Theory]
        [InlineData(0, 0, 1, 1, true)]
        [InlineData(0.1f, 0.1f, 0.9f, 0.9f, true)]
        [InlineData(-0.1f, 0, 1, 1, true)]
        [InlineData(1, 1, 1, 1, true)]
        [InlineData(1, 0, 1, 1, true)]
        [InlineData(-1, 0, 1, 1, true)]
        [InlineData(-1, -1, 3, 3, true)]
        [InlineData(-0.5f, -0.5f, 1, 1, true)]
        [InlineData(-1, -1, .5f, .5f, false)]
        [InlineData(1.1f, 1.1f, .5f, .5f, false)]
        [InlineData(1.1f, 0, .5f, .5f, false)]
        [InlineData(-1, 0, .5f, .5f, false)]
        public void IntersectsRectF(float x, float y, float width, float height, bool expected)
        {
            var sut = new RectangleF(0, 0, 1, 1);
            Assert.Equal(expected, sut.Intersects(new RectangleF(x, y, width, height)));
        }

        [Theory]
        [InlineData(0, 0, 1, 1, ContainmentType.Contains)]
        [InlineData(0.1f, 0.1f, 0.9f, 0.9f, ContainmentType.Contains)]
        [InlineData(-0.1f, 0, 1, 1, ContainmentType.Intersects)]
        [InlineData(1, 1, 1, 1, ContainmentType.Intersects)]
        [InlineData(1, 0, 1, 1, ContainmentType.Intersects)]
        [InlineData(-1, 0, 1, 1, ContainmentType.Intersects)]
        [InlineData(-1, -1, 3, 3, ContainmentType.Intersects)]
        [InlineData(-0.5f, -0.5f, 1, 1, ContainmentType.Intersects)]
        [InlineData(-1, -1, .5f, .5f, ContainmentType.Disjoint)]
        [InlineData(1.1f, 1.1f, .5f, .5f, ContainmentType.Disjoint)]
        [InlineData(1.1f, 0, .5f, .5f, ContainmentType.Disjoint)]
        [InlineData(-1, 0, .5f, .5f, ContainmentType.Disjoint)]
        public void IntersectRectF(float x, float y, float width, float height, ContainmentType expected)
        {
            var sut = new RectangleF(0, 0, 1, 1);
            Assert.Equal(expected, sut.Intersect(new RectangleF(x, y, width, height)));
        }
    }
}
