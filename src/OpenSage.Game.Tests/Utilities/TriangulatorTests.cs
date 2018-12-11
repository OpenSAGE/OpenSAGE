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

            Triangulator.Triangulate(
                polygonPoints,
                WindingOrder.CounterClockwise,
                out var trianglePoints,
                out var triangleIndices);

            Assert.Equal(4, trianglePoints.Length);

            Assert.Equal(new Vector2(-22, 577), trianglePoints[0]);
            Assert.Equal(new Vector2(-20, -694), trianglePoints[1]);
            Assert.Equal(new Vector2(1513, -696), trianglePoints[2]);
            Assert.Equal(new Vector2(1536, 968), trianglePoints[3]);

            Assert.Equal(6, triangleIndices.Length);

            Assert.Equal(0, triangleIndices[0]);
            Assert.Equal(1, triangleIndices[1]);
            Assert.Equal(3, triangleIndices[2]);
            Assert.Equal(1, triangleIndices[3]);
            Assert.Equal(2, triangleIndices[4]);
            Assert.Equal(3, triangleIndices[5]);
        }
    }
}
