using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Logic
{
    public class ColliderTests
    {
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
            var def = new ObjectDefinition
            {
                Geometry =
                {
                    Type = ObjectGeometry.Sphere,
                    Height = 10,
                    MajorRadius = radius
                }
            };

            var collider = Collider.Create(def, transform);

            Assert.Equal(expected, collider.Intersects(rect));
        }

        [Theory]
        [InlineData(0, 0, 2, 2, true)]
        public void BoxColliderRectangleFIntersections(float x, float y, float width, float height, bool expected)
        {
            var rect = new RectangleF(0, 0, 6, 4);

            var transform = new Transform(new Vector3(x, y, 0), Quaternion.Identity);
            var def = new ObjectDefinition
            {
                Geometry =
                {
                    Type = ObjectGeometry.Box,
                    Height = 10,
                    MajorRadius = width,
                    MinorRadius = height
                }
            };

            var collider = Collider.Create(def, transform);
            Assert.Equal(expected, collider.Intersects(rect));
        }
    }
}
