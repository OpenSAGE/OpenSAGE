using System.Numerics;
using OpenSage.Logic.Object;
using Xunit;

namespace OpenSage.Tests.Logic.Object;

public class GameObjectTests : MockedGameTest
{
    [Theory]
    [InlineData(0.64f, true)]
    [InlineData(0.62f, false)]
    public void IsSignificantlyAboveTerrainReturnsTrueWhenHighEnough(float height, bool expectedResult)
    {
        ZeroHour.AssetStore.GameData.Current.Gravity = -0.07f;

        var objectDefinition = new ObjectDefinition();
        var gameObject = new GameObject(objectDefinition, ZeroHour.Context, null);

        gameObject.SetTranslation(new Vector3(0, 0, height));

        Assert.Equal(expectedResult, gameObject.IsSignificantlyAboveTerrain);
    }
}
