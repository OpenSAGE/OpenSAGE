using System.Numerics;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Mathematics
{
    public class Vector2UtilityTests
    {
        [Fact]
        public void RotationByZeroIsIdentity()
        {
            var center = Vector2.Zero;
            var point = new Vector2(1, 0);
            var rotated = point.RotateAroundPoint(center, 0);
            VectorsEqual(point, rotated, 3);
        }

        [Fact]
        public void RotationBy2PiIsIdentity()
        {
            var center = Vector2.Zero;
            var point = new Vector2(30, 30);
            var rotated = point.RotateAroundPoint(center, MathUtility.TwoPi);
            VectorsEqual(point, rotated, 3);
        }

        [Fact]
        public void RotationAroundItselfIsIdentity()
        {
            var point = new Vector2(2, 2);
            var rotated = point.RotateAroundPoint(point, MathUtility.PiOver2);
            VectorsEqual(point, rotated, 3);
        }

        [Fact]
        public void RotationForwardsAndBackwardsIsIdentity()
        {
            var center = Vector2.Zero;
            var point = new Vector2(1, 3);

            var rotatedCw = point.RotateAroundPoint(center, 0.3f);
            var rotatedCcw = rotatedCw.RotateAroundPoint(center, -0.3f);
            VectorsEqual(point, rotatedCcw, 3);
        }

        [Theory]
        [InlineData(0, 0, 0, 1, 1.5708f, -1, 0)] // 90 degrees clockwise
        [InlineData(0, 0, 0, 1, -1.5708f, 1, 0)] // 90 degrees counter-clockwise
        public void RotationTheory(float axisX, float axisY, float pointX, float pointY, float angle, float resultX, float resultY)
        {
            var center = new Vector2(axisX, axisY);
            var point = new Vector2(pointX, pointY);
            var expected = new Vector2(resultX, resultY);
            var rotated = point.RotateAroundPoint(center, angle);
            VectorsEqual(expected, rotated, 3);
        }

        private static void VectorsEqual(Vector2 a, Vector2 b, int precision)
        {
            Assert.Equal(a.X, b.X, precision);
            Assert.Equal(a.Y, b.Y, precision);
        }
    }
}
