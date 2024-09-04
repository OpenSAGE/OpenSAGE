using OpenSage.Logic.Object;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Behaviors;

public class FireWeaponWhenDeadBehaviorTests : BehaviorModuleTest<FireWeaponWhenDeadBehavior, FireWeaponWhenDeadBehaviorModuleData>
{
    private static readonly byte[] GeneralsUntriggeredUpgrade = [0x01, 0x00];

    [Fact]
    public void Generals_UntriggeredUpgrade_V1()
    {
        var stream = SaveData(GeneralsUntriggeredUpgrade);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.False(behavior.UpgradeLogic.Triggered);
    }

    private static readonly byte[] GeneralsTriggeredUpgrade = [0x01, 0x01];

    [Fact]
    public void Generals_TriggeredUpgrade_V1()
    {
        var stream = SaveData(GeneralsTriggeredUpgrade);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.True(behavior.UpgradeLogic.Triggered);
    }
}
