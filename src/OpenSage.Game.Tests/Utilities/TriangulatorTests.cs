using System.Numerics;
using OpenSage.Utilities;
using Xunit;

namespace OpenSage.Tests.Utilities
{
    public class TriangulatorTests
    {
        [Fact]
        public void CanTriangulatePolygon()
        {
            var polygonPoints = new[]
            {
                new Vector2(-22, 577),
                new Vector2(-20, -694),
                new Vector2(1513, -696),
                new Vector2(1536, 968)
            };

            Assert.True(Triangulator.Process(polygonPoints, out var trianglePoints));

            Assert.Equal(6, trianglePoints.Count);

            Assert.Equal(new Vector2(1536, 968), trianglePoints[0]);
            Assert.Equal(new Vector2(-22, 577), trianglePoints[1]);
            Assert.Equal(new Vector2(-20, -694), trianglePoints[2]);
            Assert.Equal(new Vector2(-20, -694), trianglePoints[3]);
            Assert.Equal(new Vector2(1513, -696), trianglePoints[4]);
            Assert.Equal(new Vector2(1536, 968), trianglePoints[5]);
        }
    }
}
