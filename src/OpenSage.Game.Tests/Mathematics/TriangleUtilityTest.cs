using System.Collections.Generic;
using System.Numerics;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Mathematics
{
    public class TriangleUtilityTest
    {
        #region Properties
        public static IEnumerable<object[]> IsPointInsideData =>
            new List<object[]>
            {
                new object[] { new Vector2(0, 0), new Vector2(3, 0), new Vector2(0, 3), new Point2D(1, 1), true },
                new object[] { new Vector2(0, 0), new Vector2(3, 0), new Vector2(0, 3), new Point2D(2, 0), false },
                new object[] { new Vector2(0, 0), new Vector2(0, 3), new Vector2(3, 0), new Point2D(1, 1), true },
                new object[] { new Vector2(0, 0), new Vector2(3, 0), new Vector2(0, 3), new Point2D(3, 3), false },
            };
        #endregion

        #region TestMethods
        [Theory]
        [MemberData(nameof(IsPointInsideData))]
        public void IsPointInsideTest(Vector2 v1, Vector2 v2, Vector2 v3, Point2D point, bool expectedResult)
        {
            Assert.Equal(expectedResult, TriangleUtility.IsPointInside(v1,  v2,  v3,  point));
        }
        #endregion
    }
}
