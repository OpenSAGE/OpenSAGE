using OpenSage.Logic.Object;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Behaviors;

public class UpgradeModuleTest<TModule, TData> : BehaviorModuleTest<TModule, TData> where TModule : UpgradeModule where TData : UpgradeModuleData, new()
{
    private readonly byte[] _upgradeLogic = [V1, 0x00];

    protected override byte[] ModuleData() => [V1, V1, V1, .. base.ModuleData(), .. _upgradeLogic];
}
