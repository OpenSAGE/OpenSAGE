using System.Numerics;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Mathematics
{
    public class AngleTests
    {
        [Fact]
        public void DegreesToRadiansTest()
        {
            Assert.Equal(MathUtility.ToRadians(90.0f), MathUtility.PiOver2);
        }

        [Fact]
        public void RadiansToDegreesTest()
        {
            Assert.Equal(90.0f, MathUtility.ToDegrees(MathUtility.PiOver2));
        }

        [Theory]
        [InlineData(0.0f, 1.0f, 0.0f, 90.0f)]
        [InlineData(1.0f, 0.0f, 0.0f, 0.0f)]
        [InlineData(0.0f, -1.0f, 0.0f, -90.0f)]
        [InlineData(-1.0f, 0.0f, 0.0f, 180.0f)]
        public void GetYawFromDirectionTest(float x, float y, float z, float angle)
        {
            Assert.Equal(angle, MathUtility.ToDegrees(MathUtility.GetYawFromDirection(new Vector3(x, y, z))));
        }

        [Theory]
        [InlineData(0.0f, 0.0f, 1.0f, 0.0f)]
        [InlineData(0.0f, 1.0f, 0.0f, 90.0f)]
        [InlineData(0.1f, 0.1f, 0.8f, 10.024973f)]
        public void GetPitchFromDirectionTest(float x, float y, float z, float angle)
        {
            var vec = Vector3.Normalize(new Vector3(x, y, z));
            Assert.Equal(angle, MathUtility.ToDegrees(MathUtility.GetPitchFromDirection(vec)));
        }

        [Theory]
        [InlineData(90.0f, 180.0f, 90.0f)]
        [InlineData(90.0f, -180.0f, 90.0f)]
        [InlineData(90.0f, 0.0f, -90.0f)]
        public void AngleDeltaTest(float alpha, float beta, float delta)
        {
            var alphaRad = MathUtility.ToRadians(alpha);
            var betaRad = MathUtility.ToRadians(beta);
            var deltaRad = MathUtility.ToRadians(delta);
            Assert.Equal(deltaRad, MathUtility.CalculateAngleDelta(alphaRad, betaRad), 3);
        }
    }
}
