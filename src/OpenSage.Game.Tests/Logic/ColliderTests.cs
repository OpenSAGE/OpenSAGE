using Xunit;
using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Tests.Logic
{
    public class ColliderTests
    {
        [Theory]
        [InlineData(0, 0, 0, 1, true)]
        [InlineData(1, 0, 0, 1, true)]
        [InlineData(2, 0, 0, 1, true)]
        [InlineData(-2, 0, 0, 1, true)]
        [InlineData(0, -2, 0, 1, true)]
        [InlineData(0, 0, -2, 1, true)]
        [InlineData(2.01f, 0, 0, 1, false)]
        [InlineData(-2.01f, 0, 0, 1, false)]
        [InlineData(0, -2.01f, 0, 1, false)]
        [InlineData(0, 0, -2.01f, 1, false)]
        public void SphereIntersectsSphere(float x, float y, float z, float radius, bool expected)
        {
            var sphere = new SphereCollider(new Transform(Vector3.Zero, Quaternion.Identity), 1);
            var sphere2 = new SphereCollider(new Transform(new Vector3(x, y, z), Quaternion.Identity), radius);

            Assert.Equal(expected, sphere.Intersects(sphere2));
        }

        [Theory]
        [InlineData(0, 0, 1, 1, 1, true)]
        [InlineData(0.9f, 0, 1, 1, 1, true)]
        [InlineData(1, 0, 1, 1, 1, true)]
        [InlineData(0, 1, 1, 1, 1, true)]
        [InlineData(-2, 0, 1, 1, 1, true)]
        [InlineData(0, -2, 1, 1, 1, true)]
        [InlineData(1.1f, 0, 1, 1, 1, false)]
        [InlineData(0, 1.1f, 1, 1, 1, false)]
        [InlineData(-2.1f, 0, 1, 1, 1, false)]
        [InlineData(0, -2.1f, 1, 1, 1, false)]
        public void SphereIntersectsBox_AA(float x, float y, float width, float height, float boxHeight, bool expected)
        {
            var sphere = new SphereCollider(new Transform(Vector3.Zero, Quaternion.Identity), 1);
            var box = new BoxCollider(new RectangleF(x, y, width, height), boxHeight);

            Assert.Equal(expected, sphere.Intersects(box));
        }

        [Theory]
        [InlineData(-2, 0, 0, 1, 0, 0, true, 1)]
        [InlineData(0, -2, 0, 0, 1, 0, true, 1)]
        [InlineData(0, 0, -2, 0, 0, 1, true, 1)]
        [InlineData(-2, 0, 0, -1, 0, 0, false, 0)]
        [InlineData(0, -2, 0, 0, -1, 0, false, 0)]
        [InlineData(0, 0, -2, 0, 0, -1, false, 0)]
        public void SphereColliderIntersectsRay(float x, float y, float z, float dirX, float dirY, float dirZ, bool expected, float expectedDepth)
        {
            var sphere = new SphereCollider(new Transform(Vector3.Zero, Quaternion.Identity), 1);
            var ray = new Ray(new Vector3(x, y, z), new Vector3(dirX, dirY, dirZ));

            Assert.Equal(expected, sphere.Intersects(ray, out var depth));
            Assert.Equal(expectedDepth, depth);
        }

        [Theory]
        [InlineData(0, 0, 2, true)]
        [InlineData(-2, -2, 1, false)]
        [InlineData(8, 6, 1, false)]
        [InlineData(8, 6, 2, false)]
        [InlineData(8, 6, 4.3, true)]
        [InlineData(3, 2, 1, true)]
        [InlineData(3, 2, 5, true)]
        public void SphereColliderRectangleFIntersections(float x, float y, float radius, bool expected)
        {
            var rect = new RectangleF(0, 0, 6, 4);

            var transform = new Transform(new Vector3(x, y, 0), Quaternion.Identity);
            var geometryShape = new GeometryShape(new Geometry())
            {
                Type = GeometryType.Sphere,
                Height = 10,
                MajorRadius = radius
            };

            var collider = Collider.Create(geometryShape, transform);

            Assert.Equal(expected, collider.Intersects(rect));
        }

        [Theory]
        [InlineData(0, 0, 2, 2, true)]
        public void BoxColliderRectangleFIntersections(float x, float y, float width, float height, bool expected)
        {
            var rect = new RectangleF(0, 0, 6, 4);

            var transform = new Transform(new Vector3(x, y, 0), Quaternion.Identity);
            var geometryShape = new GeometryShape(new Geometry())
            {
                Type = GeometryType.Box,
                Height = 10,
                MajorRadius = width,
                MinorRadius = height
            };

            var collider = Collider.Create(geometryShape, transform);
            Assert.Equal(expected, collider.Intersects(rect));
        }
    }
}
