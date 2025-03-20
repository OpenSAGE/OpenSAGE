using OpenSage.Logic.Object;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Body;

public class InactiveBodyTests : ModuleTest
{
    [Fact]
    public void InactiveBodyHasZeroHealth()
    {
        var body = CreateBody();

        Assert.Equal(0.0f, body.Health);
        Assert.Equal(0.0f, body.MaxHealth);
        Assert.Equal(0.0f, body.InitialHealth);
        Assert.Equal(0.0f, body.PreviousHealth);
    }

    [Fact]
    public void InactiveBodyIgnoresNonUnresistableDamage()
    {
        var body = CreateBody();

        var damageOutput = body.AttemptDamage(new DamageInfoInput
        {
            DamageType = DamageType.Explosion
        });

        Assert.True(damageOutput.NoEffect);
    }

    [Fact]
    public void InactiveBodyIgnoresHealing()
    {
        var body = CreateBody();

        var damageOutput = body.AttemptHealing(new DamageInfoInput
        {
            DamageType = DamageType.Healing
        });

        Assert.True(damageOutput.NoEffect);
    }

    private InactiveBody CreateBody()
    {
        var gameObject = new GameObject(new ObjectDefinition(), ZeroHour.GameEngine, null);
        return new InactiveBody(gameObject, ZeroHour.GameEngine);
    }
}
