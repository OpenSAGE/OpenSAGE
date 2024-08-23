using OpenSage.Logic.Object;
using OpenSage.Tests.Logic.Object.Update;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Behaviors;

public class OverchargeBehaviorTests : UpdateModuleTest<OverchargeBehavior, OverchargeBehaviorModuleData>
{
    [Fact]
    public void Enabled_V1()
    {
        byte[] enabledSampleData = [0x01];

        var stream = SaveData(enabledSampleData);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.True(behavior.Enabled);
    }

    [Fact]
    public void Disabled_V1()
    {
        byte[] disabledSampleData = [0x00];

        var stream = SaveData(disabledSampleData);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.False(behavior.Enabled);
    }
}
