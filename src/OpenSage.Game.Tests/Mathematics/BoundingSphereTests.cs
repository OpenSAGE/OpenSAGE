using System;
using System.Numerics;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Mathematics
{
    public class BoundingSphereTests
    {
        [Theory]
        [InlineData(0, 0, 0, 1, 0, 0, 0)]
        [InlineData(2, 2, 2, 1, 0, 0, 0)]
        [InlineData(2, 2, 2, 2, 0, 0, 0)]
        [InlineData(2, 2, 2, 2, 2, 2, 2)]
        [InlineData(-2, .2f, .2f, 1, 0, 0, 0)]
        public void Transform(float x, float y, float z, float scale, float yaw, float pitch, float roll)
        {
            var sut = new BoundingSphere(Vector3.Zero, 1.0f);
            var translation = new Vector3(x, y, z);
            var translationMatrix = Matrix4x4.CreateTranslation(translation);
            var scaleMatrix = Matrix4x4.CreateScale(scale);
            var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);

            sut = BoundingSphere.Transform(sut, rotationMatrix * scaleMatrix * translationMatrix);

            Assert.Equal(translation, sut.Center);
            Assert.True(Math.Abs(scale - sut.Radius) < 0.001f);
        }

        [Theory]
        [InlineData(1, 0, 0, 0, PlaneIntersectionType.Intersecting)]
        [InlineData(0, 1, 0, 0, PlaneIntersectionType.Intersecting)]
        [InlineData(0, 0, 1, 0, PlaneIntersectionType.Intersecting)]
        [InlineData(-1, 0, 0, 0, PlaneIntersectionType.Intersecting)]
        [InlineData(0, -1, 0, 0, PlaneIntersectionType.Intersecting)]
        [InlineData(0, 0, -1, 0, PlaneIntersectionType.Intersecting)]
        [InlineData(1, 0, 0, 1.1f, PlaneIntersectionType.Front)]
        [InlineData(0, 1, 0, 1.1f, PlaneIntersectionType.Front)]
        [InlineData(0, 0, 1, 1.1f, PlaneIntersectionType.Front)]
        [InlineData(-1, 0, 0, -1.1f, PlaneIntersectionType.Back)]
        [InlineData(0, -1, 0, -1.1f, PlaneIntersectionType.Back)]
        [InlineData(0, 0, -1, -1.1f, PlaneIntersectionType.Back)]
        public void IntersectsPlane(float x, float y, float z, float d, PlaneIntersectionType expected)
        {
            var sut = new BoundingSphere(Vector3.Zero, 1.0f);
            var plane = new Plane(x, y, z, d);

            Assert.Equal(expected, sut.Intersects(plane));
        }

        [Theory]
        [InlineData(-1, -1, -1, 1, 1, 1, true)]
        [InlineData(-.5, -.5, -.5, .5, .5, .5, true)]
        [InlineData(-2, -2, -2, 2, 2, 2, true)]
        [InlineData(1, 0, 0, 2, 1, 1, true)]
        [InlineData(-2, 0, 0, -1, 1, 1, true)]
        [InlineData(0, -2, 0, 1, -1, 1, true)]
        [InlineData(1.1f, 0, 0, 2, 1, 1, false)]
        [InlineData(-2, 0, 0, -1.1, 1, 1, false)]
        public void IntersectsAABB(float minX, float minY, float minZ, float maxX, float maxY, float maxZ, bool expected)
        {
            var sut = new BoundingSphere(Vector3.Zero, 1.0f);
            var box = new AxisAlignedBoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));

            Assert.Equal(expected, sut.Intersects(box));
        }

        [Theory]
        [InlineData(0, 0, 0, true)]
        [InlineData(1, 0, 0, true)]
        [InlineData(0, 1, 0, true)]
        [InlineData(0, 0, 1, true)]
        [InlineData(1.1f, 0, 0, false)]
        [InlineData(0, 1.1f, 0, false)]
        [InlineData(0, 0, 1.1f, false)]
        [InlineData(-1, 0, 0, true)]
        [InlineData(0, -1, 0, true)]
        [InlineData(0, 0, -1, true)]
        [InlineData(-1.1f, 0, 0, false)]
        [InlineData(0, -1.1f, 0, false)]
        [InlineData(0, 0, -1.1f, false)]
        [InlineData(0.707, 0.707, 0, true)]
        [InlineData(0, 0.707, 0.707, true)]
        [InlineData(0.707, 0, 0.707, true)]
        [InlineData(0.71, 0.71, 0, false)]
        [InlineData(0, 0.71, 0.71, false)]
        [InlineData(0.71, 0, 0.71, false)]
        public void ContainsVec3(float x, float y, float z, bool expected)
        {
            var sut = new BoundingSphere(Vector3.Zero, 1.0f);
            Assert.Equal(expected, sut.Contains(x, y, z));
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(1, 0, true)]
        [InlineData(0, 1, true)]
        [InlineData(1.1f, 0, false)]
        [InlineData(0, 1.1f, false)]
        [InlineData(-1, 0, true)]
        [InlineData(0, -1, true)]
        [InlineData(-1.1f, 0, false)]
        [InlineData(0, -1.1f, false)]
        [InlineData(0.707, 0.707, true)]
        [InlineData(0.71, 0.71, false)]
        public void ContainsVec2(float x, float y, bool expected)
        {
            var sut = new BoundingSphere(Vector3.Zero, 1.0f);
            Assert.Equal(expected, sut.Contains(new Vector2(x, y)));
        }

        [Theory]
        [InlineData(0, 0, 0, 1,  0, 0, 0, 1,  0, 0, 0, 1)]
        [InlineData(0, 0, 0, 1,  1, 0, 0, 1,  0.5f, 0, 0, 1.5f)]
        [InlineData(0, 0, 0, 1,  2, 0, 0, 1,  1, 0, 0, 2)]
        [InlineData(-2, 0, 0, 1, 2, 0, 0, 1,  0, 0, 0, 3)]
        public void CreateMerged(float x, float y, float z, float radius,
                                float x2, float y2, float z2, float radius2,
                                float _x, float _y, float _z, float _radius)
        {
            var sphere = new BoundingSphere(new Vector3(x, y, z), radius);
            var sphere2 = new BoundingSphere(new Vector3(x2, y2, z2), radius2);

            var expected = new BoundingSphere(new Vector3(_x, _y, _z), _radius);

            var actual = BoundingSphere.CreateMerged(sphere, sphere2);

            Assert.Equal(expected.Center, actual.Center);
            Assert.Equal(expected.Radius, actual.Radius);
        }
    }
}
