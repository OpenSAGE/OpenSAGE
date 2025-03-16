
using OpenSage.Logic.Object;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Update;

public class JetAIUpdateTests : MockedGameTest
{
    [Fact]
    public void CanInstantiate()
    {
        var objectDefinition = new ObjectDefinition();
        var gameObject = new GameObject(objectDefinition, Generals.GameEngine, Generals.PlayerManager.GetPlayerByIndex(0));
        var moduleData = new JetAIUpdateModuleData();
        var update = new JetAIUpdate(gameObject, Generals.GameEngine, moduleData);
        Assert.NotNull(update);
    }
}
