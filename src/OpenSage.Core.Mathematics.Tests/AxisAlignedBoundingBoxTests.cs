using System.Collections.Generic;
using System.Numerics;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Mathematics
{
    public class AxisAlignedBoundingBoxTests
    {
        #region Properties
        public static IEnumerable<object[]> CreateFromPointsData =>
            new List<object[]>
            {
                new object[] { new List<Vector3> { new Vector3(), new Vector3(), new Vector3(), new Vector3() }, new Vector3(), new Vector3() },
                new object[] { new List<Vector3> { new Vector3(0, 0, 0), new Vector3(2, 2, 2), new Vector3(3, 3, 3), new Vector3() }, new Vector3(0, 0, 0), new Vector3(3, 3, 3) },
                new object[] { new List<Vector3> { new Vector3(-2, 0, 0), new Vector3(2, -2, 2), new Vector3(3, 3, -3), new Vector3() }, new Vector3(-2, -2, -3), new Vector3(3, 3, 2) },
            };

        public static IEnumerable<object[]> CreateMergedData =>
            new List<object[]>
            {
                new object[] { new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3() },
                new object[] { new Vector3(0, 0, 0), new Vector3(2, 2, 2), new Vector3(3, 3, 3), new Vector3(4, 4, 4), new Vector3(0, 0, 0), new Vector3(4, 4, 4) },
                new object[] { new Vector3(-2, 0, 0), new Vector3(2, 2, 2), new Vector3(3, 3, -3), new Vector3(6, 6, -2), new Vector3(-2, 0, -3), new Vector3(6, 6, 2) },
            };

        public static IEnumerable<object[]> CreateFromSphereData =>
            new List<object[]>
            {
                new object[] { new Vector3(), 0.0f, new Vector3(), new Vector3() },
                new object[] { new Vector3(2, 2, 2), 4, new Vector3(-2, -2, -2), new Vector3(6, 6, 6) },
                new object[] { new Vector3(-2, -2, -2), 6, new Vector3(-8, -8, -8), new Vector3(4, 4, 4) },
            };

        public static IEnumerable<object[]> IntersectsAxisAlignedBoxData =>
           new List<object[]>
           {
                new object[] { new Vector3(), new Vector3(), true },
                new object[] { new Vector3(0, 0, 0), new Vector3(2, 2, 2), true },
                new object[] { new Vector3(-2), new Vector3(2), true },
                new object[] { new Vector3(2.05f), new Vector3(3), false },
                new object[] { new Vector3(-4), new Vector3(-2.05f), false },
            };
        #endregion

        #region TestMethods
        [Theory]
        [MemberData(nameof(CreateFromPointsData))]
        public void CreateFromPoints(IEnumerable<Vector3> points, Vector3 expectedMin, Vector3 expectedMax)
        {
            var sut = AxisAlignedBoundingBox.CreateFromPoints(points);

            Assert.Equal(expectedMin, sut.Min);
            Assert.Equal(expectedMax, sut.Max);
        }

        [Theory]
        [MemberData(nameof(CreateMergedData))]
        public void CreateMerged(Vector3 firstMin, Vector3 firstMax, Vector3 secondMin, Vector3 secondMax, Vector3 expectedMin, Vector3 expectedMax)
        {
            var first = new AxisAlignedBoundingBox(firstMin, firstMax);
            var second = new AxisAlignedBoundingBox(secondMin, secondMax);

            var merged = AxisAlignedBoundingBox.CreateMerged(first, second);

            Assert.Equal(expectedMin, merged.Min);
            Assert.Equal(expectedMax, merged.Max);
        }

        [Theory]
        [MemberData(nameof(CreateFromSphereData))]
        public void CreateFromSphere(Vector3 center, float radius, Vector3 expectedMin, Vector3 expectedMax)
        {
            var sphere = new BoundingSphere(center, radius);

            var sut = AxisAlignedBoundingBox.CreateFromSphere(sphere);

            Assert.Equal(expectedMin, sut.Min);
            Assert.Equal(expectedMax, sut.Max);
        }

        [Theory]
        [InlineData(0, 0, 0, true)]
        [InlineData(-2, 0, 0, true)]
        [InlineData(0, -2, 0, true)]
        [InlineData(0, 0, -2, true)]
        [InlineData(2, 0, 0, true)]
        [InlineData(0, 2, 0, true)]
        [InlineData(0, 0, 2, true)]
        [InlineData(-2.05, 0, 0, false)]
        [InlineData(0, -2.05, 0, false)]
        [InlineData(0, 0, -2.05, false)]
        [InlineData(2.05, 0, 0, false)]
        [InlineData(0, 2.05, 0, false)]
        [InlineData(0, 0, 2.05, false)]
        public void Contains(float x, float y, float z, bool expected)
        {
            var sut = new AxisAlignedBoundingBox(new Vector3(-2), new Vector3(2));

            Assert.Equal(expected, sut.Contains(new Vector3(x, y, z)));
        }

        [Theory]
        [InlineData(1, 0, 0, 0, PlaneIntersectionType.Intersecting)]
        [InlineData(0, 1, 0, 0, PlaneIntersectionType.Intersecting)]
        [InlineData(1, 0, 1, 0, PlaneIntersectionType.Intersecting)]
        [InlineData(1, 0, 0, 3, PlaneIntersectionType.Front)]
        [InlineData(0, 1, 0, 3, PlaneIntersectionType.Front)]
        [InlineData(0, 0, 1, 3, PlaneIntersectionType.Front)]
        public void IntersectsPlane(float x, float y, float z, float d, PlaneIntersectionType expected)
        {
            var plane = new Plane(x, y, z, d);
            var sut = new AxisAlignedBoundingBox(new Vector3(-2), new Vector3(2));
            Assert.Equal(expected, sut.Intersects(plane));
        }

        [Theory]
        [InlineData(0, 0, 2, 2, true)]
        [InlineData(-2, -2, 2, 2, true)]
        [InlineData(2, 2, 2, 2, true)]
        [InlineData(2.05, 2.05, 2, 2, false)]
        [InlineData(-4.05, -4.05, 2, 2, false)]
        [InlineData(-4, -4, 8, 8, true)]
        [InlineData(0, 3, 2, 2, false)]
        [InlineData(0, -4.05, 2, 2, false)]
        public void InetersectsRectangleF(float x, float y, float width, float height, bool expected)
        {
            var rect = new RectangleF(x, y, width, height);
            var sut = new AxisAlignedBoundingBox(new Vector3(-2), new Vector3(2));

            Assert.Equal(expected, sut.Intersects(rect));
        }

        [Theory]
        [MemberData(nameof(IntersectsAxisAlignedBoxData))]
        public void IntersectsAxisAlignedBoundingBox(Vector3 min, Vector3 max, bool expected)
        {
            var sut = new AxisAlignedBoundingBox(new Vector3(-2), new Vector3(2));

            var other = new AxisAlignedBoundingBox(min, max);

            Assert.Equal(expected, sut.Intersects(other));
        }
        #endregion

    }
}
