using OpenSage.Logic.Object;

namespace OpenSage.Tests.Logic.Object;

public abstract class BehaviorModuleTest<TModule, TData> : ModuleTest where TModule : BehaviorModule where TData : BehaviorModuleData, new()
{
    protected override byte[] ModuleData() => [V1, V1, .. base.ModuleData()];

    protected TModule SampleModule() => (TModule)new TData().CreateModule(null!, null!);
}
