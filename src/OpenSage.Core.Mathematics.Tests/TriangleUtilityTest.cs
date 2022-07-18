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
                new object[] { new Vector2(0, 0), new Vector2(3, 0), new Vector2(0, 3), new Vector2(1, 1), true },
                new object[] { new Vector2(0, 0), new Vector2(3, 0), new Vector2(0, 3), new Vector2(2, 0), true },
                new object[] { new Vector2(0, 0), new Vector2(3, 0), new Vector2(0, 3), new Vector2(2, -0.01f), true },
                new object[] { new Vector2(0, 0), new Vector2(3, 0), new Vector2(0, 3), new Vector2(2, -0.02f), false },
                new object[] { new Vector2(0, 0), new Vector2(0, 3), new Vector2(3, 0), new Vector2(1, 1), true },
                new object[] { new Vector2(0, 0), new Vector2(3, 0), new Vector2(0, 3), new Vector2(3, 3), false },
            };
        #endregion

        #region TestMethods
        [Theory]
        [MemberData(nameof(IsPointInsideData))]
        public void IsPointInsideTest(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 point, bool expectedResult)
        {
            Assert.Equal(expectedResult, TriangleUtility.IsPointInside(v1,  v2,  v3,  point));
        }
        #endregion
    }
}
