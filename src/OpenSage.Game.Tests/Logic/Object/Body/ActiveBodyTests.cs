using OpenSage.Logic.Object;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Body;

public class ActiveBodyTests : ModuleTest
{
    [Fact]
    public void ActiveBodyHasNonZeroHealth()
    {
        var body = CreateBody(new ActiveBodyModuleData
        {
            InitialHealth = 100.0f,
            MaxHealth = 200.0f,
        });

        Assert.Equal(100.0f, body.Health);
        Assert.Equal(200.0f, body.MaxHealth);
        Assert.Equal(100.0f, body.InitialHealth);
        Assert.Equal(100.0f, body.PreviousHealth);
    }

    private ActiveBody CreateBody(ActiveBodyModuleData moduleData)
    {
        var gameObject = new GameObject(new ObjectDefinition(), ZeroHour.GameEngine, null);
        return new ActiveBody(gameObject, ZeroHour.GameEngine, moduleData);
    }
}
