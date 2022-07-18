using System.Collections.Generic;
using System.Numerics;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Mathematics
{
    public class BoundingBoxTests
    {
        #region Properties
        public static IEnumerable<object[]> IntersectsBoundingBoxData =>
           new List<object[]>
           {
                new object[] { new Vector3(), new Vector3(1), new Vector3(0), true },
                new object[] { new Vector3(3, 0, 0), new Vector3(4, 1, 1), new Vector3(0, 0, 0), false },
                new object[] { new Vector3(-2, -2, 2.05f), new Vector3(2, 2, 4), new Vector3(0, 0, 0), false },
                new object[] { new Vector3(-2, -2, 2), new Vector3(2, 2, 4), new Vector3(0, 0.3f, 0), true },
                new object[] { new Vector3(-2, -2, -4), new Vector3(2, 2, -2.05f), new Vector3(), false },
                new object[] { new Vector3(-2, -2, -4), new Vector3(2, 2, -2.05f), new Vector3(0, 0.3f, 0), true },
            };

        public static IEnumerable<object[]> IntersectsBoundingSphereData =>
           new List<object[]>
           {
                new object[] { new Vector3(), 1, true },
                new object[] { new Vector3(0, 0, 3), 1, true },
                new object[] { new Vector3(0, 0, -3), 1, true },
                new object[] { new Vector3(0, 0, 3.05f), 1, false },
                new object[] { new Vector3(0, 0, -3.05f), 1, false },
            };
        #endregion

        #region Tests
        [Theory]
        [InlineData(0, 0, 0, true)]
        [InlineData(2, 2, 2, true)]
        [InlineData(-2, -2, -2, true)]
        [InlineData(2.05f, 2.05f, 2.05f, false)]
        [InlineData(-2.05f, -2.05f, -2.05f, false)]
        public void ContainsVec3(float x, float y, float z, bool expected)
        {
            var transformation = Matrix4x4.CreateFromYawPitchRoll(1.57f, .3f, .2f);
            var aaBox = new AxisAlignedBoundingBox(new Vector3(-2), new Vector3(2));
            var transformedPoint = Vector3.Transform(new Vector3(x, y, z), transformation);
            var sut = new BoundingBox(aaBox, transformation);

            Assert.Equal(expected, sut.Contains(transformedPoint));
        }

        [Theory]
        [MemberData(nameof(IntersectsBoundingBoxData))]
        public void IntersectsBoundingBox(Vector3 min, Vector3 max, Vector3 rotation, bool expected)
        {
            var transformation = Matrix4x4.CreateFromYawPitchRoll(1.57f, 0, 0);
            var aaBox = new AxisAlignedBoundingBox(new Vector3(-2), new Vector3(2));
            var sut = new BoundingBox(aaBox, transformation);

            var transformation2 = Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
            var aaBox2 = new AxisAlignedBoundingBox(min, max);
            var other = new BoundingBox(aaBox2, transformation2);

            Assert.Equal(expected, sut.Intersects(other));
        }

        [Theory]
        [MemberData(nameof(IntersectsBoundingSphereData))]
        public void IntersectsBoundingSphere(Vector3 center, float radius, bool expected)
        {
            var transformation = Matrix4x4.CreateFromYawPitchRoll(1.57f, 0, 0);
            var aaBox = new AxisAlignedBoundingBox(new Vector3(-2), new Vector3(2));
            var sut = new BoundingBox(aaBox, transformation);

            var sphere = new BoundingSphere(center, radius);

            Assert.Equal(expected, sut.Intersects(sphere));
        }
        #endregion
    }
}
