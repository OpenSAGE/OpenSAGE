using System.IO;
using OpenSage.Logic.Object;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Behaviors;

public class UpgradeLogicTests : MockedGameTest
{
    private static UpgradeLogic SampleUpgradeLogic()
    {
        return new UpgradeLogic(new UpgradeLogicData(), () => { });
    }

    private MemoryStream SaveData(byte triggeredData)
    {
        return new MemoryStream([0x01, triggeredData]);
    }

    [Fact]
    public void UpgradeLogic_Triggered_V1()
    {
        const byte triggeredSampleData = 0x01;

        var stream = SaveData(triggeredSampleData);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleUpgradeLogic();
        behavior.Persist(reader);

        Assert.True(behavior.Triggered);
    }

    [Fact]
    public void UpgradeLogic_NotTriggered_V1()
    {
        const byte notTriggeredSampleData = 0x00;

        var stream = SaveData(notTriggeredSampleData);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleUpgradeLogic();
        behavior.Persist(reader);

        Assert.False(behavior.Triggered);
    }
}
