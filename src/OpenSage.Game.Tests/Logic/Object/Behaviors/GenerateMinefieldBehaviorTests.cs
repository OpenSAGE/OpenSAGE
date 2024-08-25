using OpenSage.Logic.Object;
using OpenSage.Tests.Logic.Object.Update;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Behaviors;

public class GenerateMinefieldBehaviorTests : BehaviorModuleTest<GenerateMinefieldBehavior, GenerateMinefieldBehaviorModuleData>
{
    /// <summary>
    /// A structure which has not yet triggered the minefield upgrade will have nothing set except for the version of the upgrade data.
    /// </summary>
    private static readonly byte[] GeneralsUntriggeredBuildingUpgrade =
        [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void Generals_UntriggeredUpgrade_V1()
    {
        var stream = SaveData(GeneralsUntriggeredBuildingUpgrade);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.False(behavior.UpgradeLogic.Triggered);
        Assert.False(behavior.Generated);
        Assert.False(behavior.Upgraded);
        Assert.False(behavior.GenerationPosition.HasValue);
        Assert.Empty(behavior.GeneratedMineIds);
    }

    /// <summary>
    /// A structure which has been triggered will have the triggered object set as triggered, as well as the matching generated flag.
    /// </summary>
    private static readonly byte[] GeneralsTriggeredBuildingUpgrade =
        [0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void Generals_TriggeredUpgrade_V1()
    {
        var stream = SaveData(GeneralsTriggeredBuildingUpgrade);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.True(behavior.UpgradeLogic.Triggered);
        Assert.True(behavior.Generated);
        Assert.False(behavior.Upgraded);
        Assert.False(behavior.GenerationPosition.HasValue);
        Assert.Empty(behavior.GeneratedMineIds);
    }

    /// <summary>
    /// A falling cluster mine bomb does not trigger until it dies, but has stored coordinates for where to generate the minefield when it does die.
    /// </summary>
    private static readonly byte[] GeneralsClusterMineBombFalling =
        [0x01, 0x00, 0x00, 0x01, 0x54, 0x92, 0x67, 0x43, 0x5b, 0x4e, 0x40, 0x44, 0x00, 0x00, 0x20, 0x41];

    [Fact]
    public void Generals_ClusterMineBomb_Falling_V1()
    {
        var stream = SaveData(GeneralsClusterMineBombFalling);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.False(behavior.UpgradeLogic.Triggered);
        Assert.False(behavior.Generated);
        Assert.False(behavior.Upgraded);
        Assert.True(behavior.GenerationPosition.HasValue);
        Assert.Equal(231.57159f, behavior.GenerationPosition.Value.X, 0.01);
        Assert.Equal(769.2243f, behavior.GenerationPosition.Value.Y, 0.01);
        Assert.Equal(10f, behavior.GenerationPosition.Value.Z, 0.01);
        Assert.Empty(behavior.GeneratedMineIds);
    }

    /// <summary>
    /// A structure which has not yet triggered the minefield upgrade will have nothing set except for the version of the upgrade data.
    /// </summary>
    private static readonly byte[] ZeroHourUntriggeredBuildingUpgrade =
        [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void ZeroHour_UntriggeredUpgrade_V1()
    {
        var stream = SaveData(ZeroHourUntriggeredBuildingUpgrade);
        var reader = new StateReader(stream, ZeroHour);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.False(behavior.UpgradeLogic.Triggered);
        Assert.False(behavior.Generated);
        Assert.False(behavior.Upgraded);
        Assert.False(behavior.GenerationPosition.HasValue);
        Assert.Empty(behavior.GeneratedMineIds);
    }

    /// <summary>
    /// A structure which has been triggered will have the triggered object set as triggered, the matching generated flag, and an array of the mines created.
    /// </summary>
    private static readonly byte[] ZeroHourTriggeredBuildingUpgrade =
        [0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0x04, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x00, 0x00, 0x0d, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00];

    [Fact]
    public void ZeroHour_TriggeredUpgrade_V1()
    {
        var stream = SaveData(ZeroHourTriggeredBuildingUpgrade);
        var reader = new StateReader(stream, ZeroHour);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.True(behavior.UpgradeLogic.Triggered);
        Assert.True(behavior.Generated);
        Assert.False(behavior.Upgraded);
        Assert.False(behavior.GenerationPosition.HasValue);
        Assert.Collection(behavior.GeneratedMineIds, id => Assert.Equal(0x04u, id), id => Assert.Equal(0x05u, id), id => Assert.Equal(0x06u, id), id => Assert.Equal(0x07u, id), id => Assert.Equal(0x08u, id), id => Assert.Equal(0x09u, id), id => Assert.Equal(0x0au, id), id => Assert.Equal(0x0bu, id), id => Assert.Equal(0x0cu, id), id => Assert.Equal(0x0du, id), id => Assert.Equal(0x0eu, id), id => Assert.Equal(0x0fu, id), id => Assert.Equal(0x10u, id));
    }

    /// <summary>
    /// A structure whose mines have been upgraded will have the triggered object set as triggered, the matching generated flag, and an array of the mines created.
    /// </summary>
    private static readonly byte[] ZeroHourUpgradedBuildingUpgrade =
        [0x01, 0x01, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0x11, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00, 0x13, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x15, 0x00, 0x00, 0x00, 0x16, 0x00, 0x00, 0x00, 0x17, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x19, 0x00, 0x00, 0x00, 0x1a, 0x00, 0x00, 0x00, 0x1b, 0x00, 0x00, 0x00, 0x1c, 0x00, 0x00, 0x00, 0x1d, 0x00, 0x00, 0x00];

    [Fact]
    public void ZeroHour_UpgradedUpgrade_V1()
    {
        var stream = SaveData(ZeroHourUpgradedBuildingUpgrade);
        var reader = new StateReader(stream, ZeroHour);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.True(behavior.UpgradeLogic.Triggered);
        Assert.True(behavior.Generated);
        Assert.True(behavior.Upgraded);
        Assert.False(behavior.GenerationPosition.HasValue);
        Assert.Collection(behavior.GeneratedMineIds, id => Assert.Equal(0x11u, id), id => Assert.Equal(0x12u, id), id => Assert.Equal(0x13u, id), id => Assert.Equal(0x14u, id), id => Assert.Equal(0x15u, id), id => Assert.Equal(0x16u, id), id => Assert.Equal(0x17u, id), id => Assert.Equal(0x18u, id), id => Assert.Equal(0x19u, id), id => Assert.Equal(0x1au, id), id => Assert.Equal(0x1bu, id), id => Assert.Equal(0x1cu, id), id => Assert.Equal(0x1du, id));
    }

    /// <summary>
    /// A falling cluster mine bomb does not trigger until it dies, but has stored coordinates for where to generate the minefield when it does die.
    /// </summary>
    private static readonly byte[] ZeroHourClusterMineBombFalling =
        [0x01, 0x00, 0x00, 0x01, 0x00, 0x03, 0x53, 0xb9, 0x43, 0x09, 0xf4, 0x2d, 0x44, 0x00, 0x00, 0x20, 0x41, 00];

    [Fact]
    public void ZeroHour_ClusterMineBomb_Falling_V1()
    {
        var stream = SaveData(ZeroHourClusterMineBombFalling);
        var reader = new StateReader(stream, ZeroHour);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.False(behavior.UpgradeLogic.Triggered);
        Assert.False(behavior.Generated);
        Assert.False(behavior.Upgraded);
        Assert.True(behavior.GenerationPosition.HasValue);
        Assert.Equal(370.64853f, behavior.GenerationPosition.Value.X, 0.01);
        Assert.Equal(695.81305f, behavior.GenerationPosition.Value.Y, 0.01);
        Assert.Equal(10f, behavior.GenerationPosition.Value.Z, 0.01);
        Assert.Empty(behavior.GeneratedMineIds);
    }
}
